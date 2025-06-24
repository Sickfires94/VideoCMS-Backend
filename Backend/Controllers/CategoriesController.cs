using Backend.DTOs;
using Backend.Services.Interfaces; // Your ICategoryService
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

    // --- Helper Methods for Manual Mapping ---

    /// <summary>
    /// Maps a single Category EF entity to a flat CategoryResponseDto.
    /// </summary>
    private CategoryResponseDto MapToCategoryResponseDto(Category category)
    {
        if (category == null) return null;
        return new CategoryResponseDto
        {
            CategoryId = category.categoryId,
            CategoryName = category.categoryName,
            CategoryParentId = category.categoryParentId,
            // Access parent's name if CategoryParent navigation property is loaded
            ParentCategoryName = category.categoryParent?.categoryName
        };
    }

    /// <summary>
    /// Maps a single Category EF entity to a CategoryTreeItemDto, including its children recursively.
    /// This assumes children are already eager-loaded on the input 'category' entity.
    /// </summary>
    private CategoryTreeItemDto MapToCategoryTreeItemDto(Category category)
    {
        if (category == null) return null;

        var dto = new CategoryTreeItemDto
        {
            CategoryId = category.categoryId,
            CategoryName = category.categoryName,
            CategoryParentId = category.categoryParentId,
            ParentCategoryName = category.categoryParent?.categoryName,
            Children = new List<CategoryTreeItemDto>()
        };

        // Recursively map children if they exist and are loaded
        if (category.children != null && category.children.Any())
        {
            foreach (var child in category.children.OrderBy(c => c.categoryName)) // Order children for consistent output
            {
                dto.Children.Add(MapToCategoryTreeItemDto(child));
            }
        }
        return dto;
    }

    // --- Controller Actions ---

    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<CategoryResponseDto>> CreateCategory([FromBody] Category category)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdCategory = await _categoryService.CreateCategoryAsync(category); // This returns EF Category entity
        if (createdCategory == null)
        {
            return Conflict("Category name is not unique under the specified parent, or parent category does not exist.");
        }

        // FIX: Manually map the EF entity to the response DTO
        var responseDto = MapToCategoryResponseDto(createdCategory);
        return CreatedAtAction(nameof(GetCategoryById), new { categoryId = responseDto.CategoryId }, responseDto);
    }

    [HttpGet("get/{categoryId}")]
    [ProducesResponseType(typeof(CategoryResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CategoryResponseDto>> GetCategoryById(int categoryId)
    {
        // Service method should ensure CategoryParent is eager-loaded if ParentCategoryName is desired
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (category == null)
        {
            return NotFound();
        }
        // FIX: Manually map to DTO
        var responseDto = MapToCategoryResponseDto(category);
        return Ok(responseDto);
    }

    [HttpGet("getByName/{categoryName}")]
    [ProducesResponseType(typeof(CategoryResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CategoryResponseDto>> GetCategoryByName(string categoryName)
    {
        // Service method should ensure CategoryParent is eager-loaded if ParentCategoryName is desired
        var category = await _categoryService.GetCategoryByNameAsync(categoryName);
        if (category == null) return NotFound();

        // FIX: Manually map to DTO
        var responseDto = MapToCategoryResponseDto(category);
        return Ok(responseDto);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponseDto>), 200)] // Using flat DTO for all categories
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAllCategories()
    {
        // Service method should ensure CategoryParent is eager-loaded if ParentCategoryName is desired
        var categories = await _categoryService.GetAllCategoriesAsync(); // Returns IEnumerable<Category>
        // FIX: Manually map each entity to DTO
        var dtoList = categories.Select(c => MapToCategoryResponseDto(c)).ToList();
        return Ok(dtoList);
    }

    /// <summary>
    /// Retrieves all immediate children of a specified parent category.
    /// </summary>
    /// <param name="parentCategoryId">The ID of the parent category.</param>
    /// <returns>200 OK with a list of immediate children.</returns>
    [HttpGet("children/{parentCategoryId}")]
    [ProducesResponseType(typeof(IEnumerable<CategoryTreeItemDto>), 200)]
    public async Task<ActionResult<IEnumerable<CategoryTreeItemDto>>> GetImmediateChildren(int parentCategoryId)
    {
        // Service method should eager load children and potentially parents if needed for TreeItemDto
        var children = await _categoryService.GetImmediateChildrenAsync(parentCategoryId);
        // FIX: Manually map to CategoryTreeItemDto, building the tree structure
        var dtoList = children.Select(c => MapToCategoryTreeItemDto(c)).ToList();
        return Ok(dtoList);
    }

    /// <summary>
    /// Retrieves all descendant categories (children, grandchildren, etc.) of a given category recursively.
    /// </summary>
    /// <param name="categoryId">The ID of the starting category.</param>
    /// <returns>200 OK with a list of descendant categories.</returns>
    [HttpGet("descendants/{categoryId}")]
    [ProducesResponseType(typeof(IEnumerable<CategoryTreeItemDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<CategoryTreeItemDto>>> GetDescendantCategories(int categoryId)
    {
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (category == null)
        {
            return NotFound("Starting category not found.");
        }

        // Service method should handle loading all descendants, potentially with their children for tree representation
        var descendants = await _categoryService.GetDescendantCategoriesAsync(categoryId);
        // FIX: Manually map to CategoryTreeItemDto
        var dtoList = descendants.Select(c => MapToCategoryTreeItemDto(c)).ToList(); // Note: This might flatten if GetDescendantCategoriesAsync doesn't return a pre-built tree structure
        return Ok(dtoList);
    }

    /// <summary>
    /// Retrieves the hierarchical path from the root to the given category.
    /// </summary>
    /// <param name="categoryId">The ID of the category.</param>
    /// <returns>200 OK with a list of categories representing the hierarchy.</returns>
    [HttpGet("hierarchy/{categoryId}")]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategoryHierarchy(int categoryId)
    {
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (category == null)
        {
            return NotFound("Category for hierarchy not found.");
        }

        // Service method should return a flat list of categories from root to target
        var hierarchy = await _categoryService.GetCategoryHierarchyAsync(categoryId);
        // FIX: Manually map to CategoryResponseDto
        var dtoList = hierarchy.Select(c => MapToCategoryResponseDto(c)).ToList();
        return Ok(dtoList);
    }

    [HttpGet("search/{categoryName}")]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponseDto>), 200)]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> SearchCategoriesByName(string categoryName)
    {
        // Service method should ensure CategoryParent is eager-loaded if ParentCategoryName is desired
        var categories = await _categoryService.SearchCategoriesByName(categoryName);
        // FIX: Manually map to DTO list
        var dtoList = categories.Select(c => MapToCategoryResponseDto(c)).ToList();
        return Ok(dtoList);
    }

    [HttpGet("Top")]
    [ProducesResponseType(typeof(IEnumerable<CategoryTreeItemDto>), 200)]
    public async Task<ActionResult<IEnumerable<CategoryTreeItemDto>>> GetTopCategories()
    {
        // Service method should eager load children and potentially parents if needed for TreeItemDto
        // Assuming GetTopLevelCategoriesAsync returns top-level categories with their children eager-loaded.
        var categories = await _categoryService.GetTopLevelCategoriesAsync();
        // FIX: Manually map to CategoryTreeItemDto
        var dtoList = categories.Select(c => MapToCategoryTreeItemDto(c)).ToList();
        return Ok(dtoList);
    }

    [HttpGet("Tree")]
    [ProducesResponseType(typeof(IEnumerable<CategoryTreeItemDto>), 200)]
    public async Task<ActionResult<IEnumerable<CategoryTreeItemDto>>> GetAllCategoriesWithChildren()
    {
        // This method is designed to return a full tree.
        // The service method MUST eager load children (e.g., .Include(c => c.Children))
        // for the MapToCategoryTreeItemDto to build the recursive structure.
        IEnumerable<Category> categories = await _categoryService.GetAllCategoriesWithChildrenAsync();
        Debug.WriteLine($"Received {categories.Count()} top-level categories for tree.");
        // FIX: Manually map to CategoryTreeItemDto
        var dtoList = categories.Select(c => MapToCategoryTreeItemDto(c)).ToList();
        return Ok(dtoList);
    }
}