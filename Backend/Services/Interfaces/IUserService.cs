
using Backend.DTOs; // Assuming your DTOs are here

namespace Backend.Services.Interface
{
    public interface IUserService
    {
        Task<AuthenticatedUserDto?> AuthenticateUserAsync(User user);

        Task<User?> RegisterUserAsync(User user);

        Task<User?> GetUserByIdAsync(int userId);

        Task<IEnumerable<User>> GetAllUsersAsync();

        Task<User?> UpdateUserProfileAsync(User user);

        Task<bool> DeleteUserAsync(int userId);

        Task<bool> ChangePasswordAsync(int userId, string password);

        Task<bool> CheckUsernameExistsAsync(User user);

        Task<bool> CheckEmailExistsAsync(User user);
    }
}