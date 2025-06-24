using Backend.DTOs;

namespace Backend.Services.Interfaces
{
    public interface ICategoryService
    {
        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="category">The Category DTO to create.</param>
        /// <returns>The created Category if successful, otherwise null (e.g., if name is not unique or parent does not exist).</returns>
        Task<Category?> CreateCategoryAsync(Category category);

        /// <summary>
        /// Retrieves a category by its unique ID.
        /// </summary>
        /// <param name="categoryId">The ID of the category to retrieve.</param>
        /// <returns>The Category DTO if found, otherwise null.</returns>
        Task<Category?> GetCategoryByIdAsync(int categoryId);

        Task<Category?> GetCategoryByNameAsync(string categoryName);
        Task<List<Category?>> SearchCategoriesByName(string categoryName);

        /// <summary>
        /// Retrieves all categories.
        /// </summary>
        /// <returns>An enumerable collection of all Category DTOs.</returns>
        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        /// <summary>
        /// Retrieves all direct children of a given parent category.
        /// </summary>
        /// <param name="parentCategoryId">The ID of the parent category.</param>
        /// <returns>An enumerable collection of immediate child Category DTOs.</returns>
        Task<IEnumerable<Category>> GetImmediateChildrenAsync(int parentCategoryId);

        /// <summary>
        /// Retrieves all descendants (children, grandchildren, etc.) of a given category recursively.
        /// </summary>
        /// <param name="categoryId">The ID of the starting category.</param>
        /// <returns>An enumerable collection of all descendant Category DTOs.</returns>
        Task<IEnumerable<Category>> GetDescendantCategoriesAsync(int categoryId);

        /// <summary>
        /// Retrieves the hierarchical path from the root to the given category.
        /// </summary>
        /// <param name="categoryId">The ID of the category for which to retrieve the hierarchy.</param>
        /// <returns>An enumerable collection of Category DTOs representing the parent hierarchy, starting from the root.</returns>
        Task<IEnumerable<Category>> GetCategoryHierarchyAsync(int categoryId);

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="category">The Category DTO with updated values. The categoryId must be valid.</param>
        /// <returns>The updated Category DTO if successful, otherwise null (e.g., if not found, or updated name is not unique).</returns>
        Task<Category?> UpdateCategoryAsync(Category category);

        /// <summary>
        /// Deletes a category by its ID.
        /// </summary>
        /// <param name="categoryId">The ID of the category to delete.</param>
        /// <returns>True if the category was successfully deleted, false if not found or if it has children.</returns>
        Task<bool> DeleteCategoryAsync(int categoryId);

        /// <summary>
        /// Checks if a category name is unique within its parent scope.
        /// </summary>
        /// <param name="categoryName">The name to check for uniqueness.</param>
        /// <param name="parentId">The ID of the parent category. Null for root categories.</param>
        /// <param name="excludeCategoryId">Optional. If provided, this category ID will be excluded from the uniqueness check (useful for updates).</param>
        /// <returns>True if the name is unique within the specified parent scope, false otherwise.</returns>
        Task<bool> IsCategoryNameUniqueAsync(string categoryName, int? parentId, int? excludeCategoryId = null);
        Task<IEnumerable<Category>> GetTopLevelCategoriesAsync();
        Task<IEnumerable<Category>> GetAllCategoriesWithChildrenAsync();
        
    }
}
