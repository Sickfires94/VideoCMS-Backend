using Backend.DTOs;
using Backend.Services;
using Backend.Services.Interface;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] Category category)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdCategory = await _categoryService.CreateCategoryAsync(category);
        if (createdCategory == null)
        {
            return Conflict("Category name is not unique under the specified parent, or parent category does not exist.");
        }
        return CreatedAtAction(nameof(GetCategoryById), new { categoryId = createdCategory.categoryId }, createdCategory);
    }

    [HttpGet("{categoryId}")]
    public async Task<IActionResult> GetCategoryById(int categoryId)
    {
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (category == null)
        {
            return NotFound();
        }
        return Ok(category);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(categories);
    }

    [HttpPut("{categoryId}")]
    public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] Category category)
    {
        if (categoryId != category.categoryId)
        {
            return BadRequest("Category ID in URL does not match body.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedCategory = await _categoryService.UpdateCategoryAsync(category);
        if (updatedCategory == null)
        {
            return NotFound("Category not found, or updated name is not unique under the specified parent, or new parent does not exist.");
        }
        return Ok(updatedCategory);
    }

    [HttpDelete("{categoryId}")]
    public async Task<IActionResult> DeleteCategory(int categoryId)
    {
        bool deleted = await _categoryService.DeleteCategoryAsync(categoryId);
        if (!deleted)
        {
            // This could mean not found, or it has children.
            // You might want more specific messages if the client needs to distinguish.
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                return NotFound("Category not found.");
            }
            else // Category exists but has children
            {
                return Conflict("Cannot delete category as it has existing children.");
            }
        }
        return NoContent(); // Successfully deleted
    }

    /// <summary>
    /// Retrieves all immediate children of a specified parent category.
    /// </summary>
    /// <param name="parentCategoryId">The ID of the parent category.</param>
    /// <returns>200 OK with a list of immediate children.</returns>
    [HttpGet("children/{parentCategoryId}")]
    public async Task<IActionResult> GetImmediateChildren(int parentCategoryId)
    {
        var children = await _categoryService.GetImmediateChildrenAsync(parentCategoryId);
        // It's possible for a parent to exist but have no children, in which case an empty list is returned, which is still 200 OK.
        // If you want to return 404 if the parentCategory doesn't exist at all, you'd need an extra check here:
        // var parentExists = await _categoryService.GetCategoryByIdAsync(parentCategoryId);
        // if (parentExists == null && children.Any() == false) return NotFound("Parent category not found.");
        return Ok(children);
    }

    /// <summary>
    /// Retrieves all descendant categories (children, grandchildren, etc.) of a given category recursively.
    /// </summary>
    /// <param name="categoryId">The ID of the starting category.</param>
    /// <returns>200 OK with a list of descendant categories.</returns>
    [HttpGet("descendants/{categoryId}")]
    public async Task<IActionResult> GetDescendantCategories(int categoryId)
    {
        // Check if the initial category exists
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (category == null)
        {
            return NotFound("Starting category not found.");
        }

        var descendants = await _categoryService.GetDescendantCategoriesAsync(categoryId);
        return Ok(descendants);
    }

    /// <summary>
    /// Retrieves the hierarchical path from the root to the given category.
    /// </summary>
    /// <param name="categoryId">The ID of the category.</param>
    /// <returns>200 OK with a list of categories representing the hierarchy.</returns>
    [HttpGet("hierarchy/{categoryId}")]
    public async Task<IActionResult> GetCategoryHierarchy(int categoryId)
    {
        // Check if the initial category exists.
        // The hierarchy method in service returns from root to current,
        // so if current category doesn't exist, the list would be empty or incorrect.
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (category == null)
        {
            return NotFound("Category for hierarchy not found.");
        }

        var hierarchy = await _categoryService.GetCategoryHierarchyAsync(categoryId);
        // The hierarchy will be from root to the specified category.
        return Ok(hierarchy);
    }
}