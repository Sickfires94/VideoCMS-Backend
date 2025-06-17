using Backend.DTOs;
using Backend.Services.Interfaces;
using Backend.Services.VideoMetaDataServices;
using Backend.Services.VideoMetaDataServices.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.VideoControllers
{

    [ApiController]
    [Route("/api/video/search")]
    public class VideoMetadataSearchController
    {

        private readonly IVideoMetadataSearchService _searchService;

        public VideoMetadataSearchController(IVideoMetadataSearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("/get/{name}")]
        public async Task<ICollection<VideoMetadata>> GetByName(string name)
        {
            return await _searchService.searchVideoMetadataByName(name);
        }

    }
}
