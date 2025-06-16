using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
