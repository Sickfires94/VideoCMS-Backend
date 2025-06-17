using Backend.DTOs;
using Backend.Services;
using Backend.Services.VideoMetaDataServices;
using Backend.Services.VideoMetaDataServices.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.VideoControllers
{


    [ApiController]
    [Route("/api/video")]
    public class VideoMetadataController : ControllerBase
    {
        private readonly IVideoMetadataService _videoMetadataService;

        public VideoMetadataController(IVideoMetadataService videoMetadataService)
        {
            _videoMetadataService = videoMetadataService;
        }

        [HttpGet]
        public async Task<IEnumerable<VideoMetadata>> Get()
        {
            return await _videoMetadataService.getAllVideoMetadata();
        }

        [HttpPost]
        public async Task<VideoMetadata> Post([FromBody] VideoMetadata video, [FromBody] List<int> tagsId)
        {
            /// TODO insert adding tags logic after implementing tags 
            return await _videoMetadataService.addVideoMetadata(video);
        }

    }
}
