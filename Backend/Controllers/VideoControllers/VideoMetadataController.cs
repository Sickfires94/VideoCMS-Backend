using Backend.DTOs;
using Backend.DTOs.RequestDtos;
using Backend.DTOs.ResponseDtos;
using Backend.Services;
using Backend.Services.Mappers.Interfaces;
using Backend.Services.VideoMetaDataServices;
using Backend.Services.VideoMetaDataServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Backend.Controllers.VideoControllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class VideoMetadataController : ControllerBase
    {
        private readonly IVideoMetadataService _videoMetadataService;
        private readonly IVideoMetadataMapperService _videoMetadataMapperService;
        private readonly ILogger<VideoMetadataController> _logger;

        public VideoMetadataController(IVideoMetadataService videoMetadataService, ILogger<VideoMetadataController> logger, IVideoMetadataMapperService videoMetadataMapperService)
        {
            _videoMetadataService = videoMetadataService;
            _videoMetadataMapperService = videoMetadataMapperService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            Debug.WriteLine("Writing Log");
            _logger.LogInformation("Retrieving all videos");
            return Ok(await _videoMetadataService.getAllVideoMetadata());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] VideoMetadataRequestDto request)
        {
            VideoMetadata video = _videoMetadataMapperService.ToEntity(request);
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

            VideoMetadataResponseDto response = _videoMetadataMapperService.ToResponse(video);

            return Ok(response);
        }

        [Authorize]
        [HttpPost("{videoId}")]
        public async Task<IActionResult> UpdateVideoMetadata(int videoId, [FromBody] VideoMetadataRequestDto request)
        {
            // Map to Entity
            VideoMetadata video = _videoMetadataMapperService.ToEntity(request);

            // Update Entity
            VideoMetadata updatedVideoMetadata;
            try
            {
                updatedVideoMetadata = await _videoMetadataService.updateVideoMetadata(videoId, video);
            }

            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine("Logging and returning unauthorized");
                _logger.LogInformation("Unauthorized update action for video ID: {videoId}", videoId);
                return Unauthorized("Unauthorized Action: You do not have permission to Update the video");
            }

            if (updatedVideoMetadata == null)
            {
                return NotFound("Video metadata not found for the given ID.");
            }

            // Map to response DTO
            VideoMetadataResponseDto response = _videoMetadataMapperService.ToResponse(video);

            return Ok(response);
        }

        [Authorize]
        [HttpDelete("{videoId}")]
        public async Task<IActionResult> Delete(int videoId)
        {
            try
            {
                await _videoMetadataService.deleteVideoMetadata(videoId);
                return Ok("Video metadata deleted successfully.");
            }
            catch (UnauthorizedAccessException ex) { 
                _logger.LogInformation("Unauthorized Delete action for video ID: {videoId}", videoId);
                return Unauthorized("Unauthorized Action: You do not have permission to delete the video");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting video metadata: {ex.Message}");
            }
        }

    }
}
