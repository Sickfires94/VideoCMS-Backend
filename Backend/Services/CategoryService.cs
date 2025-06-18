using Backend.DTOs;
using Backend.Repositories.Interface;
using Backend.Services.Interface;
using Backend.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Creates a new category after validating its uniqueness and parent existence.
        /// </summary>
        /// <param name="category">The Category DTO to create.</param>
        /// <returns>The created Category if successful, otherwise null.</returns>
        public async Task<Category?> CreateCategoryAsync(Category category)
        {
            // 1. Validate Parent Category existence if a parentId is provided
            if (category.categoryParentId.HasValue)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(category.categoryParentId.Value);
                if (parentCategory == null)
                {
                    // Parent category does not exist, cannot create this category
                    return null;
                }
            }

            // 2. Check for name uniqueness within the parent scope
            bool isUnique = await IsCategoryNameUniqueAsync(category.categoryName, category.categoryParentId);
            if (!isUnique)
            {
                // A category with the same name already exists under the same parent
                return null;
            }

            // 3. Create the category via the repository
            var createdCategory = await _categoryRepository.CreateAsync(category);
            return createdCategory;
        }

        /// <summary>
        /// Retrieves a category by its ID.
        /// </summary>
        /// <param name="categoryId">The ID of the category.</param>
        /// <returns>The Category DTO or null if not found.</returns>
        public async Task<Category?> GetCategoryByIdAsync(int categoryId)
        {
            return await _categoryRepository.GetByIdAsync(categoryId);
        }

        /// <summary>
        /// Retrieves all categories.
        /// </summary>
        /// <returns>An enumerable collection of all categories.</returns>
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        /// <summary>
        /// Retrieves all immediate children of a specified parent category.
        /// </summary>
        /// <param name="parentCategoryId">The ID of the parent category.</param>
        /// <returns>An enumerable collection of immediate children.</returns>
        public async Task<IEnumerable<Category>> GetImmediateChildrenAsync(int parentCategoryId)
        {
            return await _categoryRepository.GetAllImmediateChildrenAsync(parentCategoryId);
        }

        /// <summary>
        /// Retrieves all descendant categories (children, grandchildren, etc.) of a given category.
        /// This method leverages the repository's recursive fetching capability.
        /// </summary>
        /// <param name="categoryId">The ID of the starting category.</param>
        /// <returns>An enumerable collection of all descendant categories.</returns>
        public async Task<IEnumerable<Category>> GetDescendantCategoriesAsync(int categoryId)
        {
            // Note: The CategoryRepository's GetAllChildrenAsync is already recursive.
            // If the recursion depth is very high, consider iterative approaches
            // or specialized database queries (e.g., CTEs) for performance.
            return await _categoryRepository.GetAllChildrenAsync(categoryId);
        }

        /// <summary>
        /// Retrieves the hierarchical path from the root to the given category.
        /// </summary>
        /// <param name="categoryId">The ID of the category.</param>
        /// <returns>An enumerable collection of Category DTOs representing the hierarchy.</returns>
        public async Task<IEnumerable<Category>> GetCategoryHierarchyAsync(int categoryId)
        {
            return await _categoryRepository.GetAllParentsHierarchyAsync(categoryId);
        }

        /// <summary>
        /// Updates an existing category after validating its uniqueness and parent existence.
        /// </summary>
        /// <param name="category">The Category DTO with updated values.</param>
        /// <returns>The updated Category if successful, otherwise null.</returns>
        public async Task<Category?> UpdateCategoryAsync(Category category)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(category.categoryId);
            if (existingCategory == null)
            {
                // Category not found
                return null;
            }

            // 1. Validate Parent Category existence if parentId is changed and provided
            if (category.categoryParentId.HasValue && existingCategory.categoryParentId != category.categoryParentId)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(category.categoryParentId.Value);
                if (parentCategory == null)
                {
                    // New parent category does not exist
                    return null;
                }
            }

            // 2. Check for name uniqueness if name or parentId is being changed
            // Exclude the current category's ID from the uniqueness check
            if (existingCategory.categoryName != category.categoryName || existingCategory.categoryParentId != category.categoryParentId)
            {
                bool isUnique = await IsCategoryNameUniqueAsync(category.categoryName, category.categoryParentId, category.categoryId);
                if (!isUnique)
                {
                    // New category name is not unique under the specified parent
                    return null;
                }
            }

            // 3. Update the existing entity properties
            existingCategory.categoryName = category.categoryName;
            existingCategory.categoryParentId = category.categoryParentId;
            // categoryParent will be loaded by EF Core if needed, no need to manually set it here for update.

            // 4. Save changes to the database
            await _categoryRepository.SaveChangesAsync();
            return existingCategory;
        }

        /// <summary>
        /// Deletes a category by its ID, but only if it has no children.
        /// </summary>
        /// <param name="categoryId">The ID of the category to delete.</param>
        /// <returns>True if deleted, false if not found or has children.</returns>
        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            var categoryToDelete = await _categoryRepository.GetByIdAsync(categoryId);
            if (categoryToDelete == null)
            {
                // Category not found
                return false;
            }

            // Prevent deletion if the category has children
            bool hasChildren = await _categoryRepository.HasChildrenAsync(categoryId);
            if (hasChildren)
            {
                // Business rule: Cannot delete categories with existing children.
                // You might choose to re-parent children or cascade delete based on requirements.
                return false;
            }

            await _categoryRepository.DeleteAsync(categoryId);
            return true;
        }

        /// <summary>
        /// Checks if a category name is unique within its parent scope.
        /// If parentId is null, it checks for uniqueness among root categories.
        /// If excludeCategoryId is provided, it's used to exclude the current category during update checks.
        /// </summary>
        /// <param name="categoryName">The name to check.</param>
        /// <param name="parentId">The parent ID (null for root).</param>
        /// <param name="excludeCategoryId">ID of category to exclude from check.</param>
        /// <returns>True if unique, false otherwise.</returns>
        public async Task<bool> IsCategoryNameUniqueAsync(string categoryName, int? parentId, int? excludeCategoryId = null)
        {
            // Get all categories that match the name (case-insensitive)
            var existingCategoriesWithName = (await _categoryRepository.GetAllAsync())
                                            .Where(c => c.categoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

            // Filter by parentId
            if (parentId.HasValue)
            {
                existingCategoriesWithName = existingCategoriesWithName.Where(c => c.categoryParentId == parentId.Value);
            }
            else // Checking for root category uniqueness
            {
                existingCategoriesWithName = existingCategoriesWithName.Where(c => !c.categoryParentId.HasValue);
            }

            // Exclude the category being updated if excludeCategoryId is provided
            if (excludeCategoryId.HasValue)
            {
                existingCategoriesWithName = existingCategoriesWithName.Where(c => c.categoryId != excludeCategoryId.Value);
            }

            // If no categories match after all filters, the name is unique
            return !existingCategoriesWithName.Any();
        }
    }
}
