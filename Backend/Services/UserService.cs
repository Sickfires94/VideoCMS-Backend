using Backend.DTOs;
using Backend.Repositories.Interface;
using Backend.Services.Interface;
using Backend.Services.Interfaces;
using BCrypt.Net; // Add this using directive for BCrypt
using System; // For DateOnly
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService; // Inject ITokenService

        public UserService(IUserRepository userRepository, ITokenService tokenService) // Update constructor
        {
            _userRepository = userRepository;
            _tokenService = tokenService; // Assign injected service
        }

        public async Task<AuthenticatedUserDto?> AuthenticateUserAsync(User user) // Return type now AuthenticatedUserDto
        {
            var storedUser = await _userRepository.GetByEmailAsync(user.userEmail); // Assumed method name

            if (storedUser != null && BCrypt.Net.BCrypt.Verify(user.userPassword, storedUser.userPassword))
            {
                // User authenticated, now generate JWT token using the dedicated service
                // Pass the actual User entity to the TokenService
                var token = _tokenService.GenerateJwtToken(storedUser); // Pass Backend.Models.User

                return new AuthenticatedUserDto
                {
                    userId = storedUser.userId,
                    userName = storedUser.userName,
                    userEmail = storedUser.userEmail,
                    userCreatedDate = storedUser.userCreatedDate,
                    userUpdatedDate = storedUser.userUpdatedDate,
                    role = storedUser.role, // Populate the Role from the storedUser entity
                    Token = token
                };
            }
            return null; // Authentication failed
        }




        public async Task<User?> RegisterUserAsync(User user)
        {
            if (await _userRepository.IsEmailUniqueAsync(user.userEmail) == false)
            {
                return null; // Email already exists
            }

            // Hash the password before saving
            user.userPassword = BCrypt.Net.BCrypt.HashPassword(user.userPassword);

            var createdUser = await _userRepository.CreateAsync(user);
            return createdUser;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User?> UpdateUserProfileAsync(User user)
        {
            var existingUser = await _userRepository.GetByIdAsync(user.userId);
            if (existingUser == null)
            {
                return null; // User not found
            }

            // Check if email or username are being changed and if they remain unique
            if (existingUser.userEmail != user.userEmail && await _userRepository.IsEmailUniqueAsync(user.userEmail) == false)
            {
                return null; // New email already exists
            }

            if (existingUser.userName != user.userName && await _userRepository.IsUsernameUniqueAsync(user.userName) == false)
            {
                return null; // New username already exists
            }

            // Update only allowed fields. Password is handled by ChangePasswordAsync.
            existingUser.userName = user.userName;
            existingUser.userEmail = user.userEmail;
            existingUser.userUpdatedDate = DateTime.UtcNow; // Update the modified date

            await _userRepository.SaveChangesAsync(); // Save the changes
            return existingUser;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var userToDelete = await _userRepository.GetByIdAsync(userId);
            if (userToDelete == null)
            {
                return false; // User not found
            }

            await _userRepository.DeleteAsync(userToDelete);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
        {
            var userToUpdate = await _userRepository.GetByIdAsync(userId);
            if (userToUpdate == null)
            {
                return false; // User not found
            }

            // Hash the new password before saving
            userToUpdate.userPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
            userToUpdate.userUpdatedDate = DateTime.UtcNow; // Update the modified date

            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckUsernameExistsAsync(User user)
        {
            return !(await _userRepository.IsUsernameUniqueAsync(user.userName));
        }

        public async Task<bool> CheckEmailExistsAsync(User user)
        {
            return !(await _userRepository.IsEmailUniqueAsync(user.userEmail));
        }
    }
}