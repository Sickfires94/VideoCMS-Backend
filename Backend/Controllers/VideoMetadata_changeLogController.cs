using Backend.Repositories.Interface;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VideoMetadata_changeLogController : ControllerBase
    {
        private readonly IVideoMetadata_changeLogService _service;

        public VideoMetadata_changeLogController(IVideoMetadata_changeLogService service)
        {
            _service = service;
        }

        [HttpGet("{videoId}")]
        public async Task<IActionResult> GetLogsByVideoId(int videoId)
        {
            return Ok(await _service.getLogsByVideoId(videoId));
        }
    }
}
