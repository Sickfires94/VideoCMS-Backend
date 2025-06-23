namespace Backend.Services.Interfaces
{
    public interface IBlobStorageService
    {
        public Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        public Task<Stream> DownloadFileAsync(string fileName);
        public Task<List<string>> ListBlobsAsync();
        public Task<bool> DeleteFileAsync(string fileName);
        public string GetBlobUrl(string fileName);

        public Task<string> GenerateUploadSasUriAsync(string fileName, int expiryMinutes);
        public Task<string> GenerateDownloadSasUriFromUrlAsync(string blobUrl, int expiryMinutes = 60);
    }
}
