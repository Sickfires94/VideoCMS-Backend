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
        public async Task<string> GenerateDownloadSasUriAsync(string fileName, int expiryMinutes = 60)
        {
            // Get a reference to the specific blob client using the injected container client
            BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);

            // Important: Check if the blob actually exists before generating a download SAS
            // This prevents generating a valid SAS for a non-existent blob.
            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException($"Blob '{fileName}' not found for SAS token generation.");
            }

            // Create a SAS builder for the blob
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = _blobContainerClient.Name, // Use the name from the client directly
                BlobName = fileName, // Apply SAS to this specific blob
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes),
                StartsOn = DateTimeOffset.UtcNow,
                Resource = "b" // "b" for blob (object-level SAS)
            };

            // --- Grant ONLY Read permission for downloading ---
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Check if the client can generate SAS URIs (i.e., has the right credentials)
            if (!blobClient.CanGenerateSasUri)
            {
                throw new InvalidOperationException("BlobClient cannot generate SAS URI for download. Ensure the underlying client was configured with appropriate credentials (e.g., StorageSharedKeyCredential or Azure AD with User Delegation Key).");
            }

            // Generate the SAS URI
            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);

            return sasUri.ToString(); // Return the full URI including the SAS token
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
