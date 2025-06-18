using Backend.DTOs;
using Backend.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] Tag tag)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdTag = await _tagService.CreateTagAsync(tag);
        if (createdTag == null)
        {
            return Conflict("Tag name is not unique.");
        }
        return CreatedAtAction(nameof(GetTagById), new { tagId = createdTag.tagId }, createdTag);
    }

    [HttpGet("{tagId}")]
    public async Task<IActionResult> GetTagById(int tagId)
    {
        var tag = await _tagService.GetTagByIdAsync(tagId);
        if (tag == null)
        {
            return NotFound();
        }
        return Ok(tag);
    }

    [HttpGet("name/{tagName}")] // Example endpoint for getting by name
    public async Task<IActionResult> GetTagByName(string tagName)
    {
        var tag = await _tagService.GetTagByNameAsync(tagName);
        if (tag == null)
        {
            return NotFound();
        }
        return Ok(tag);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTags()
    {
        var tags = await _tagService.GetAllTagsAsync();
        return Ok(tags);
    }

    [HttpPut("{tagId}")]
    public async Task<IActionResult> UpdateTag(int tagId, [FromBody] Tag tag)
    {
        if (tagId != tag.tagId)
        {
            return BadRequest("Tag ID in URL does not match body.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedTag = await _tagService.UpdateTagAsync(tag);
        if (updatedTag == null)
        {
            // Check if it's not found or a uniqueness conflict
            var existing = await _tagService.GetTagByIdAsync(tagId);
            if (existing == null)
            {
                return NotFound("Tag not found.");
            }
            else
            {
                return Conflict("Updated tag name is not unique.");
            }
        }
        return Ok(updatedTag);
    }

    [HttpDelete("{tagId}")]
    public async Task<IActionResult> DeleteTag(int tagId)
    {
        bool deleted = await _tagService.DeleteTagAsync(tagId);
        if (!deleted)
        {
            return NotFound("Tag not found."); // Currently, the service only returns false if not found.
        }
        return NoContent(); // Successfully deleted
    }
}