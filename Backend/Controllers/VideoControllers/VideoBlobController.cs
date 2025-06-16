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

    }
}
