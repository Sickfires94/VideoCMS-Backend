using Backend.Repositories.VideoMetadataRepositories.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.VideoControllers
{

    [ApiController]
    [Route("/api/video/blob")]
    public class VideoBlobController : ControllerBase
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly IVideoMetadataRepository _videoMetadataRepository;

        public VideoBlobController(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }


        [HttpPost("/upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            using (var stream = file.OpenReadStream())
            {
                string url = await _blobStorageService.UploadFileAsync(stream, file.FileName, file.ContentType);
                return Ok(new { Message = "File uploaded successfully!", Url = url });
            }
        }

        [HttpGet("/download/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            var stream = await _blobStorageService.DownloadFileAsync(fileName);
            if (stream == null)
            {
                return NotFound("File not found.");
            }
            // You might want to get the content type from the blob properties if needed
            return File(stream, "application/octet-stream", fileName); // Adjust content type
        }

        [HttpGet("/list")]
        public async Task<IActionResult> List()
        {
            var blobs = await _blobStorageService.ListBlobsAsync();
            return Ok(blobs);
        }

        [HttpDelete("/delete/{fileName}")]
        public async Task<IActionResult> Delete(string fileName)
        {
            bool deleted = await _blobStorageService.DeleteFileAsync(fileName);
            if (deleted)
            {
                return Ok($"File '{fileName}' deleted successfully.");
            }
            return NotFound($"File '{fileName}' not found or could not be deleted.");
        }

        [HttpGet("generate-upload-sas")] // This action will be accessible at /api/Blob/generate-upload-sas
        public async Task<ActionResult<string>> GenerateUploadSas([FromQuery] string fileName)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest("File name cannot be empty.");
            }

            // In a real application, implement robust authorization here.
            // Example: [Authorize(Roles = "Uploader")]
            // Or check if the current user has permission to upload this specific file type/name.
            // For instance, you might prepend a user ID to the fileName:
            // string uniqueFileName = $"{User.Identity.Name}/{fileName}"; // If user is authenticated

            try
            {
                // Set an appropriate expiry time. 5-15 minutes is common for uploads.
                // It should be long enough for the upload to complete but short enough to limit exposure.
                const int sasExpiryMinutes = 10;
                string sasUri = await _blobStorageService.GenerateUploadSasUriAsync(fileName, sasExpiryMinutes);

                return Ok(sasUri);
            }
            catch (InvalidOperationException ex)
            {
                // This exception might be thrown by your service if BlobClient.CanGenerateSasUri is false
                return StatusCode(500, "Server configuration error: Unable to generate SAS token. Please check storage credentials.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal server error occurred while processing your request.");
            }
        }

        [HttpGet("generate-download-sas")]
        public async Task<ActionResult<string>> GenerateDownloadSas([FromQuery] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest("File name cannot be empty.");
            }

            try
            {
                // Ensure GenerateDownloadSasUriAsync method exists in IBlobStorageService
                // (You'd need to add it if it's not there, similar to GenerateUploadSasUriAsync)
                string sasUri = await _blobStorageService.GenerateDownloadSasUriAsync(fileName, 60); // Read for 60 mins
                return Ok(sasUri);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error generating download link.");
            }
        }
    }
}
