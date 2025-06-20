using Backend.DTOs;
using Backend.Services.VideoMetaDataServices.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Backend.Controllers.VideoControllers
{

    [ApiController]
    [Route("/api/video/search")]
    public class VideoMetadataSearchController : ControllerBase
    {

        private readonly IVideoMetadataSearchService _searchService;

        public VideoMetadataSearchController(IVideoMetadataSearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet] // Now handles GET requests to the base route /api/video/search
        public async Task<ActionResult<SearchVideoMetadataResponse>> Search([FromQuery] string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(new List<VideoMetadata>());
            }

            ICollection<VideoMetadataIndexDTO> query_result = await _searchService.SearchVideoMetadata(query); // Assuming a more generic search method now
            Debug.WriteLine("Query Count: " + query_result.Count);
            SearchVideoMetadataResponse response = new(query_result);

            Debug.WriteLine("Response items count" + response.items.Count);
            return Ok(response);
        }

    }
}
