using Backend.DTOs;

namespace Backend.Repositories.Interface
{
    public interface ICategoryRepository
    {
        Task<Category> CreateAsync(Category category);
        Task DeleteAsync(int categoryId);
        Task<Category> GetByIdAsync(int id);
        Task<Category> GetByNameAsync(string name);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<IEnumerable<Category>> GetAllImmediateChildrenAsync(int categoryId);
        Task<IEnumerable<Category>> GetAllChildrenAsync(int categoryId);
        Task<IEnumerable<Category>> GetAllParentsHierarchyAsync(int categoryId);
        Task SaveChangesAsync();
        Task<bool> HasChildrenAsync(int categoryId);

    }
}
