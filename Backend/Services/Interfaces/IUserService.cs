
using Backend.DTOs;
using Backend.DTOs.ResponseDtos; // Assuming your DTOs are here

namespace Backend.Services.Interface
{
    public interface IUserService
    {
        Task<AuthenticatedUserResponseDto?> AuthenticateUserAsync(User user);

        Task<User?> RegisterUserAsync(User user);

        Task<User?> GetUserByIdAsync(int userId);

        Task<IEnumerable<User>> GetAllUsersAsync();

        Task<bool> CheckUsernameExistsAsync(string userName);

        Task<bool> CheckEmailExistsAsync(string userEmail);
    }
}