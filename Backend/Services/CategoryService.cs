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

        public async Task<Category?> GetCategoryByIdAsync(int categoryId)
        {
            return await _categoryRepository.GetByIdAsync(categoryId);
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Category>> GetImmediateChildrenAsync(int parentCategoryId)
        {
            return await _categoryRepository.GetAllImmediateChildrenAsync(parentCategoryId);
        }

        public async Task<IEnumerable<Category>> GetDescendantCategoriesAsync(int categoryId)
        {
            // Note: The CategoryRepository's GetAllChildrenAsync is already recursive.
            // If the recursion depth is very high, consider iterative approaches
            // or specialized database queries (e.g., CTEs) for performance.
            return await _categoryRepository.GetAllChildrenAndSelfAsync(categoryId);
        }

       public async Task<IEnumerable<Category>> GetCategoryHierarchyAsync(int categoryId)
        {
            return await _categoryRepository.GetAllParentsHierarchyAsync(categoryId);
        }

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

        public async Task<bool> IsCategoryNameUniqueAsync(string categoryName, int? parentId, int? excludeCategoryId = null)
        {
            // Get all categories that match the name (case-insensitive)
            var category = (await _categoryRepository.GetByNameAsync(categoryName));

            return category == null;
        }

        public async Task<Category?> GetCategoryByNameAsync(string categoryName)
        {
            var category = await _categoryRepository.GetByNameAsync(categoryName);
            return category;

        }

        public async Task<List<Category>?> SearchCategoriesByName(string categoryName)
        {
            var categories = await _categoryRepository.GetCategoryListByNameAsync(categoryName);
            return categories;
        }

        public async Task<IEnumerable<Category>> GetTopLevelCategoriesAsync()
        {
            return await _categoryRepository.GetTopLevelCategoriesAsync();
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesWithChildrenAsync()
        {
            return await _categoryRepository.GetAllCategoriesWithChildren();
        }


    }
}
