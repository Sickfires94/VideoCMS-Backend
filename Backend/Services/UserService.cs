using Backend.DTOs;
using Backend.DTOs.ResponseDtos;
using Backend.Repositories.Interface;
using Backend.Services.Interface;
using Backend.Services.Interfaces;
using BCrypt.Net; // Add this using directive for BCrypt
using Microsoft.IdentityModel.Tokens;
using System; // For DateOnly
using System.Collections.Generic;
using System.Diagnostics;
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

        public async Task<AuthenticatedUserResponseDto?> AuthenticateUserAsync(User user) // Return type now AuthenticatedUserDto
        {
            var storedUser = await _userRepository.GetByEmailAsync(user.userEmail); // Assumed method name

            if (storedUser != null && BCrypt.Net.BCrypt.Verify(user.userPassword, storedUser.userPassword))
            {
                var token = _tokenService.GenerateJwtToken(storedUser); // Pass Backend.Models.User

                return new AuthenticatedUserResponseDto
                {
                    userId = storedUser.userId,
                    userName = storedUser.userName,
                    userEmail = storedUser.userEmail,
                    userCreatedDate = storedUser.userCreatedDate,
                    role = storedUser.role, // Populate the Role from the storedUser entity
                    token = token
                };
            }
            return null; // Authentication failed
        }




        public async Task<User?> RegisterUserAsync(User user)
        {

            /// TODO throw errors here for controller to catch
            if (await CheckEmailExistsAsync(user.userEmail)) return null;

            if (await CheckUsernameExistsAsync(user.userName)) return null;


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

        public async Task<bool> CheckUsernameExistsAsync(string userName)
        {
            return !(await _userRepository.IsUsernameUniqueAsync(userName));
        }

        public async Task<bool> CheckEmailExistsAsync(string userEmail)
        {
            return !(await _userRepository.IsEmailUniqueAsync(userEmail));
        }
    }
}