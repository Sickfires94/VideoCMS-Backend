using Backend.DTOs;

namespace Backend.Repositories.Interface
{
    public interface ICategoryRepository
    {
        Task<Category> CreateAsync(Category category);
        Task DeleteAsync(int categoryId);
        Task<Category> GetByIdAsync(int id);
        Task<Category> GetByNameAsync(string name);
        Task<List<Category>> GetCategoryListByNameAsync(string name);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<IEnumerable<Category>> GetAllImmediateChildrenAsync(int categoryId);
        Task<IEnumerable<Category>> GetAllChildrenAndSelfAsync(int categoryId);
        Task<IEnumerable<Category>> GetAllParentsHierarchyAsync(int categoryId);
        Task SaveChangesAsync();
        Task<bool> HasChildrenAsync(int categoryId);
        Task<IEnumerable<Category>> GetTopLevelCategoriesAsync();
        Task<IEnumerable<Category>> GetAllCategoriesWithChildren();

    }
}
