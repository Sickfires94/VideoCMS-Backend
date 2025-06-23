using Backend.DTOs;
using Backend.Services;
using Backend.Services.VideoMetaDataServices;
using Backend.Services.VideoMetaDataServices.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.VideoControllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class VideoMetadataController : ControllerBase
    {
        private readonly IVideoMetadataService _videoMetadataService;

        public VideoMetadataController(IVideoMetadataService videoMetadataService)
        {
            _videoMetadataService = videoMetadataService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _videoMetadataService.getAllVideoMetadata());
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] VideoMetadata video)
        {
            return Ok(await _videoMetadataService.addVideoMetadata(video));
        }

        [HttpGet("{videoId}")]
        public async Task<IActionResult> GetById(int videoId)
        {
            VideoMetadata video = await _videoMetadataService.getVideoMetadataById(videoId);
            if(video == null)
            {
                return NotFound("Video By Id does not exist");
            }

            return Ok(video);
        }

        [HttpPost("{videoId}")]
        public async Task<IActionResult> UpdateVideoMetadata(int videoId, [FromBody] VideoMetadata videoMetadata)
        {
            var updatedVideoMetadata = await _videoMetadataService.updateVideoMetadata(videoId, videoMetadata);
            if (updatedVideoMetadata == null)
            {
                return NotFound("Video metadata not found for the given ID.");
            }
            return Ok(updatedVideoMetadata);
        }

        [HttpDelete("{videoId}")]
        public async Task<IActionResult> Delete(int videoId)
        {
            try
            {
                await _videoMetadataService.deleteVideoMetadata(videoId);
                return Ok("Video metadata deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting video metadata: {ex.Message}");
            }
        }

    }
}
