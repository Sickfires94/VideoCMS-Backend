using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Backend.Configurations.DataConfigs;
using Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Backend.Services
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly string _containerName;

        public AzureBlobStorageService(BlobContainerClient blobContainerClient, IOptions<AzureStorageConfig> config)
        {
            _blobContainerClient = blobContainerClient;
            _containerName = config.Value.ContainerName; // Get container name from config
        }

        // --- Uploading a File ---
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            // Get a reference to a blob
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);

            // Upload the file
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });

            // Return the URI of the uploaded blob
            return blobClient.Uri.ToString();
        }

        // --- Downloading a File ---
        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);
            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadContentAsync();
                return response.Value.Content.ToStream();
            }
            return null; // Or throw an exception
        }

        // --- Listing Blobs in a Container ---
        public async Task<List<string>> ListBlobsAsync()
        {
            List<string> blobNames = new List<string>();
            await foreach (BlobItem blobItem in _blobContainerClient.GetBlobsAsync())
            {
                blobNames.Add(blobItem.Name);
            }
            return blobNames;
        }

        public async Task<string> GenerateUploadSasUriAsync(string fileName, int expiryMinutes = 10)
        {
            // Ensure the container exists. This is typically done on app startup in DI,
            // but harmless to check again if you want to be extra safe.
            await _blobContainerClient.CreateIfNotExistsAsync();

            // Get a reference to the specific blob client using the injected container client
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);

            // Create a SAS builder for the blob
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = _blobContainerClient.Name, // Use the name from the client directly
                BlobName = fileName, // Apply SAS to this specific blob
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes),
                StartsOn = DateTimeOffset.UtcNow,
                Resource = "b" // "b" for blob
            };

            // Grant write permissions for uploading.
            sasBuilder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create | BlobSasPermissions.Add);

            // Generate the SAS URI.
            // This relies on the _blobContainerClient (and thus blobClient derived from it)
            // having been initialized with a credential that allows SAS generation (e.g., SharedKeyCredential).
            if (!blobClient.CanGenerateSasUri)
            {
                throw new InvalidOperationException("BlobClient cannot generate SAS URI. Ensure the underlying BlobServiceClient or BlobContainerClient was configured with a StorageSharedKeyCredential or Azure AD credentials that permit SAS generation.");
            }

            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);

            return sasUri.ToString(); // Return the full URI including the SAS token
        }

        /// <summary>
        /// Generates a Service SAS URI for downloading a specific blob.
        /// The client will use this URI to directly download from Azure Blob Storage.
        /// </summary>
        /// <param name="fileName">The name of the blob to download.</param>
        /// <param name="expiryMinutes">How long the SAS token should be valid for (e.g., 60 minutes).</param>
        /// <returns>A string containing the full SAS URI for downloading.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the specified blob does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the BlobClient is not configured to generate SAS URIs.</exception>
        public async Task<string> GenerateDownloadSasUriFromUrlAsync(string blobUrl, int expiryMinutes = 60)
        {
            if (string.IsNullOrWhiteSpace(blobUrl))
            {
                throw new ArgumentException("Blob URL cannot be empty.", nameof(blobUrl));
            }

            string blobName;
            try
            {

                Uri uri;
                try
                {
                    uri = new Uri(blobUrl);
                }
                catch (UriFormatException e)
                {
                    return "";
                }

                string path = uri.AbsolutePath; // e.g., "/videos/Google%20newe%20Gemini%20%E2%80%94%20Mozilla%20Firefox%202025-06-12%2004-05-25.mp4"

                Console.WriteLine($"DEBUG: Full URL received: {blobUrl}");
                Console.WriteLine($"DEBUG: URI AbsolutePath: {path}");
                Console.WriteLine($"DEBUG: Configured Container Name: '{_blobContainerClient.Name}'");

                // --- Improved blobName extraction ---
                // Find the index of the container name in the path.
                // Ensures we get the part *after* the container name, including potential subfolders.
                // Example path: /videos/subfolder/myblob.mp4
                // Expected segment: /videos/
                var containerPathSegment = $"/{_blobContainerClient.Name}/";
                int containerPathIndex = path.IndexOf(containerPathSegment, StringComparison.OrdinalIgnoreCase);

                if (containerPathIndex == -1)
                {
                    // This scenario is for blobs directly in the container root like `/containerName/fileName.mp4`
                    // Check if the path ends exactly with the container name (e.g., /videos) followed by a file name
                    // and not just /videos itself.
                    if (path.Equals($"/{_blobContainerClient.Name}", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException($"Provided blob URL '{blobUrl}' points only to the container root, not a specific blob. Expected a blob name after the container name (e.g., /videos/myblob.mp4).", nameof(blobUrl));
                    }
                    else if (path.StartsWith($"/{_blobContainerClient.Name}", StringComparison.OrdinalIgnoreCase))
                    {
                        // Path starts with /containerName but doesn't have the trailing slash of the segment
                        // This implies the blob is directly in the root of the container
                        // Example: /videos/myblob.mp4
                        blobName = path.Substring($"/{_blobContainerClient.Name}".Length).TrimStart('/');
                    }
                    else
                    {
                        throw new ArgumentException($"Provided blob URL '{blobUrl}' does not contain the expected container path segment '/{_blobContainerClient.Name}/'. Check if the URL is for the correct container or if it includes a subpath.", nameof(blobUrl));
                    }
                }
                else
                {
                    // Path includes subfolders, e.g., /videos/folder/myblob.mp4
                    blobName = path.Substring(containerPathIndex + containerPathSegment.Length);
                }

                // IMPORTANT: Uri.AbsolutePath already decodes URL-encoded characters.
                // The blobName extracted here should be the *decoded* name.
                // E.g., "Google newe Gemini — Mozilla Firefox 2025-06-12 04-05-25.mp4"
                Console.WriteLine($"DEBUG: Extracted Blob Name (decoded): '{blobName}'");

                if (string.IsNullOrWhiteSpace(blobName))
                {
                    throw new ArgumentException($"Could not extract a valid blob name from URL: {blobUrl}", nameof(blobUrl));
                }
            }
            catch (UriFormatException ex)
            {
                throw new ArgumentException("Invalid blob URL format.", nameof(blobUrl), ex);
            }
            catch (ArgumentException) // Re-throw our custom ArgumentExceptions
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to parse blob name from URL '{blobUrl}': {ex.Message}", nameof(blobUrl), ex);
            }

            // Get the BlobClient from the _blobContainerClient using the extracted blobName.
            BlobClient blobClient = _blobContainerClient.GetBlobClient(blobName);

            // Important: Now, check if the blob actually exists using the correctly credentialed BlobClient
            Console.WriteLine($"DEBUG: Attempting ExistsAsync() for blob name: '{blobClient.Name}' in container '{blobClient.BlobContainerName}'.");
            bool blobExists = await blobClient.ExistsAsync();
            Console.WriteLine($"DEBUG: Blob ExistsAsync() result: {blobExists}");

            if (!blobExists)
            {
                Console.WriteLine($"DEBUG: Final check: Blob name '{blobClient.Name}' NOT FOUND. Confirm this exact name is in Azure Blob Storage.");
                throw new FileNotFoundException($"Blob '{blobClient.Name}' (from URL: {blobUrl}) not found for SAS token generation. This filename should match exactly what is in Azure Blob Storage.");
            }

            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = _blobContainerClient.Name,
                BlobName = blobClient.Name, // Use blobClient.Name for consistency
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes),
                StartsOn = DateTimeOffset.UtcNow,
                Resource = "b"
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            if (!blobClient.CanGenerateSasUri)
            {
                throw new InvalidOperationException("The BlobClient (derived from BlobContainerClient) cannot generate SAS URI. This suggests the BlobContainerClient itself is not configured with credentials that permit SAS generation (e.g., SharedKeyCredential or User Delegation Key).");
            }

            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);

            return sasUri.ToString();
        }
        


        // --- Deleting a File ---
        public async Task<bool> DeleteFileAsync(string fileName)
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);
            return await blobClient.DeleteIfExistsAsync();
        }

        // --- Get Blob URL ---
        public string GetBlobUrl(string fileName)
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }
    }
}
