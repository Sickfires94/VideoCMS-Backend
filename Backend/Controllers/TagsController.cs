using Backend.DTOs;
using Backend.DTOs.RequestDtos;
using Backend.DTOs.ResponseDtos;
using Backend.Services.Interface;
using Backend.Services.Interfaces;
using Backend.Services.Mappers.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly ITagMapperService _tagMapperService;
    private readonly IGenerateTagsService _generateTagsService;
    private readonly ILogger<TagsController> _logger;

    public TagsController(ITagService tagService, IGenerateTagsService generateTagsService, ITagMapperService tagMapperService, ILogger<TagsController> logger)
    {
        _tagService = tagService;
        _generateTagsService = generateTagsService;
        _tagMapperService = tagMapperService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] TagRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Tag tag = _tagMapperService.toEntity(request);

        Tag createdTag = await _tagService.CreateTagAsync(tag);
        if (createdTag == null)
        {
            return Conflict("Tag name is not unique.");
        }
        TagResponseDto response = _tagMapperService.toResponse(createdTag);

        return CreatedAtAction(nameof(GetTagById), new { tagId = createdTag.tagId }, response);
    }

    [HttpGet("{tagId}")]
    public async Task<IActionResult> GetTagById(int tagId)
    {
        Tag tag = await _tagService.GetTagByIdAsync(tagId);
        if (tag == null)
        {
            return NotFound();
        }

        TagResponseDto response = _tagMapperService.toResponse(tag);
        return Ok(response);
    }

    [HttpGet("name/{tagName}")] // Example endpoint for getting by name
    public async Task<IActionResult> GetTagByName(string tagName)
    {
        Tag tag = await _tagService.GetTagByNameAsync(tagName);
        if (tag == null)
        {
            return NotFound();
        }

        TagResponseDto response = _tagMapperService.toResponse(tag);
        return Ok(tag);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTags()
    {
        IEnumerable<Tag> tags = await _tagService.GetAllTagsAsync();
        IEnumerable<TagResponseDto> response = tags.Select(t => _tagMapperService.toResponse(t)).ToList();
        
        return Ok(response);
    }


    [HttpDelete("{tagId}")]
    public async Task<IActionResult> DeleteTag(int tagId)
    {
        bool deleted = await _tagService.DeleteTagAsync(tagId);
        if (!deleted)
        {
            return NotFound("Tag not found."); 
        }
        return Ok();
    }

    [HttpGet("generate")]
    public async Task<IActionResult> GenerateTags(string title, string description)
    {
        IEnumerable<string> tagNames = await _generateTagsService.GenerateTags(title, description);
        IEnumerable<TagResponseDto> response = tagNames.Select(t => _tagMapperService.toResponse(t));
        return Ok(response);
    }
}