using Backend.DTOs;

namespace Backend.Repositories.Interface
{
    public interface IUserRepository
    {

        Task<User> CreateAsync(User user);
        Task DeleteAsync(User user);
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> IsEmailUniqueAsync(string email);
        Task<bool> IsUsernameUniqueAsync(string username);
        Task SaveChangesAsync();
    }
}
