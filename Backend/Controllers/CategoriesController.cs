using AutoMapper;
using Backend.DTOs;
using Backend.DTOs.RequestDtos;
using Backend.DTOs.ResponseDtos.Categories;
using Backend.Services.Interfaces; // Your ICategoryService
using Backend.Services.Mappers.Interfaces;
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
    private readonly ICategoryMapperService _categoryMapperService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ICategoryMapperService categoryMapperService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _categoryMapperService = categoryMapperService;
        _logger = logger;
    }

    // --- Controller Actions ---

    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> CreateCategory([FromBody] CategoryRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        Category category = _categoryMapperService.toEntity(request);

        var createdCategory = await _categoryService.CreateCategoryAsync(category); // This returns EF Category entity
        
        if (createdCategory == null)
        {
            return Conflict("Category name is not unique under the specified parent, or parent category does not exist.");
        }

        CategoryResponseDto response = _categoryMapperService.toResponse(category);

        return CreatedAtAction(nameof(GetCategoryById), new { categoryId = createdCategory.categoryId }, response);
    }

    [HttpGet("get/{categoryId}")]
    public async Task<ActionResult<CategoryResponseDto>> GetCategoryById(int categoryId)
    {
        // Service method should ensure CategoryParent is eager-loaded if ParentCategoryName is desired
        Category? category = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (category == null)
        {
            _logger.LogInformation("Attempted to fetch non-existing catogory by id: {categoryId}", categoryId);
            return NotFound();
        }

        CategoryResponseDto response = _categoryMapperService.toResponse(category);
        return Ok(response);
    }

    [HttpGet("getByName/{categoryName}")]
    public async Task<ActionResult<CategoryResponseDto>> GetCategoryByName(string categoryName)
    {
        // Service method should ensure CategoryParent is eager-loaded if ParentCategoryName is desired
        Category category = await _categoryService.GetCategoryByNameAsync(categoryName);
        if (category == null) return NotFound();

        // FIX: Manually map to DTO
        CategoryResponseDto response = _categoryMapperService.toResponse(category);
        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetAllCategories()
    {
        IEnumerable<Category> categories = await _categoryService.GetAllCategoriesAsync();

        IEnumerable<CategoryResponseDto> dtoList = categories.Select(c => _categoryMapperService.toResponse(c)).ToList();
        return Ok(dtoList);
    }

    [HttpGet("children/{parentCategoryId}")]
    public async Task<ActionResult<IEnumerable<CategoryTreeItemDto>>> GetImmediateChildren(int parentCategoryId)
    {
        IEnumerable<Category> children = await _categoryService.GetImmediateChildrenAsync(parentCategoryId);
        IEnumerable<CategoryResponseDto> dtoList = children.Select(c => _categoryMapperService.toResponse(c)).ToList();
        return Ok(dtoList);
    }

    /// <summary>
    /// Retrieves all descendant categories (children, grandchildren, etc.) of a given category recursively.
    /// </summary>
    /// <param name="categoryId">The ID of the starting category.</param>
    /// <returns>200 OK with a list of descendant categories.</returns>
    [HttpGet("descendants/{categoryId}")]
    public async Task<ActionResult<IEnumerable<CategoryTreeItemDto>>> GetDescendantCategories(int categoryId)
    {
        Category category = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (category == null)
        {
            return NotFound("Category not found.");
        }

        // Service method should handle loading all descendants, potentially with their children for tree representation
        IEnumerable<Category> descendants = await _categoryService.GetDescendantCategoriesAsync(categoryId);
        // FIX: Manually map to CategoryTreeItemDto
        IEnumerable<CategoryResponseDto> dtoList = descendants.Select(c => _categoryMapperService.toResponse(c)).ToList(); // Note: This might flatten if GetDescendantCategoriesAsync doesn't return a pre-built tree structure
        return Ok(dtoList);
    }

    [HttpGet("hierarchy/{categoryId}")]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategoryHierarchy(int categoryId)
    {
        Category category = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (category == null)
        {
            return NotFound("Category for hierarchy not found.");
        }

        IEnumerable<Category> hierarchy = await _categoryService.GetCategoryHierarchyAsync(categoryId);
        IEnumerable<CategoryResponseDto> dtoList = hierarchy.Select(c => _categoryMapperService.toResponse(c)).ToList();
        return Ok(dtoList);
    }

    [HttpGet("search/{categoryName}")]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> SearchCategoriesByName(string categoryName)
    {
        IEnumerable<Category> categories = await _categoryService.SearchCategoriesByName(categoryName);
        IEnumerable<CategoryResponseDto> dtoList = categories.Select(c => _categoryMapperService.toResponse(c)).ToList();
        return Ok(dtoList);
    }


    /// TODO REFACTOR TO USE SERVICE INSTEAD OF CONTROLLER
    //[HttpGet("Top")]
    //public async Task<ActionResult<IEnumerable<CategoryTreeItemDto>>> GetTopCategories()
    //{
    //   var categories = await _categoryService.GetTopLevelCategoriesAsync();
    //    // FIX: Manually map to CategoryTreeItemDto
    //    var dtoList = categories.Select(c => MapToCategoryTreeItemDto(c)).ToList();
    //    return Ok(dtoList);
    //}

    /// TODO REFACTOR TO USE SERVICE INSTEAD OF CONTROLLER
    //[HttpGet("Tree")]
    //public async Task<ActionResult<IEnumerable<CategoryTreeItemDto>>> GetAllCategoriesWithChildren()
    //{
    //    IEnumerable<Category> categories = await _categoryService.GetAllCategoriesWithChildrenAsync();
    //    Debug.WriteLine($"Received {categories.Count()} top-level categories for tree.");
    //    // FIX: Manually map to CategoryTreeItemDto
    //    var dtoList = categories.Select(c => MapToCategoryTreeItemDto(c)).ToList();
    //    return Ok(dtoList);
    //}
}