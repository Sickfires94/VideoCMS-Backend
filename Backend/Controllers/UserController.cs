using Backend.DTOs;
using Backend.Services.Interface; // Assuming IUserService is in this namespace
using Microsoft.AspNetCore.Authorization; // For [Authorize] attribute
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="userDto">The user data for registration (username, password, email).</param>
        /// <returns>201 Created if successful, 400 Bad Request if input is invalid, 409 Conflict if user/email already exists.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userDto)
        {
            // Basic validation using Data Annotations defined in the User DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Returns 400 with validation errors
            }

            var registeredUser = await _userService.RegisterUserAsync(userDto);
            if (registeredUser == null)
            {
                // The service returns null if username or email already exists or parent does not exist
                // We'll try to distinguish the reason for a more specific message
                if (await _userService.CheckEmailExistsAsync(userDto))
                {
                    return Conflict("Registration failed: Email address is already registered.");
                }
                // If not email conflict, then it's likely a username conflict
                else if (await _userService.CheckUsernameExistsAsync(userDto))
                {
                    return Conflict("Registration failed: Username is already taken.");
                }
                else
                {
                    return BadRequest("Registration failed due to an unknown issue.");
                }
            }
            // Return 201 Created and include the location of the newly created resource
            return CreatedAtAction(nameof(GetUserById), new { userId = registeredUser.userId }, registeredUser);
        }

        /// <summary>
        /// Authenticates a user and simulates a successful login.
        /// (In a real app, this would return a JWT token).
        /// </summary>
        /// <param name="loginCredentials">User's email and password for authentication.</param>
        /// <returns>200 OK with user info if authenticated, 401 Unauthorized if credentials are invalid.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginCredentials)
        {
            Debug.WriteLine("Reached inside controller");

            if (string.IsNullOrWhiteSpace(loginCredentials.userEmail) || string.IsNullOrWhiteSpace(loginCredentials.userPassword))
            {
                return BadRequest("Email and password are required.");
            }


            var authenticatedUser = await _userService.AuthenticateUserAsync(loginCredentials);
            if (authenticatedUser == null)
            {
                return Unauthorized("Invalid email or password."); // 401 Unauthorized
            }

            return Ok(new { Message = "Login successful!", User = new { authenticatedUser.userId, authenticatedUser.userName, authenticatedUser.userEmail, authenticatedUser.Token } });
        }

        /// <summary>
        /// Retrieves a user by their ID. Requires authentication.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>200 OK with the user data, or 404 Not Found.</returns>
        [HttpGet("{userId}")]
        [Authorize] // Requires a valid JWT token
        public async Task<IActionResult> GetUserById(int userId)
        {
            if (User.Identity.IsAuthenticated && User.FindFirst(ClaimTypes.NameIdentifier)?.Value != userId.ToString()) return Forbid();

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }
            // Return user details, but consider omitting sensitive info like password
            return Ok(new { user.userId, user.userName, user.userEmail, user.userCreatedDate, user.userUpdatedDate });
        }

        /// <summary>
        /// Retrieves all users. Requires authentication and administrative privileges.
        /// </summary>
        /// <returns>200 OK with a list of all users.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")] // Example: Only users with "Admin" role can access
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            // Map to a DTO that excludes passwords before returning
            var userList = users.Select(u => new { u.userId, u.userName, u.userEmail, u.userCreatedDate, u.userUpdatedDate });
            return Ok(userList);
        }

        /// <summary>
        /// Updates an authenticated user's profile.
        /// </summary>
        /// <param name="userId">The ID of the user to update (from URL).</param>
        /// <param name="userDto">The updated user data.</param>
        /// <returns>200 OK with updated user, 400 Bad Request, 404 Not Found, or 409 Conflict.</returns>
        [HttpPut("{userId}")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile(int userId, [FromBody] User userDto)
        {
            if (userId != userDto.userId)
            {
                return BadRequest("User ID in URL does not match ID in body.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Optional: Ensure the authenticated user is updating their own profile
            // var authenticatedUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            // if (authenticatedUserId != userId) return Forbid();

            var updatedUser = await _userService.UpdateUserProfileAsync(userDto);
            if (updatedUser == null)
            {
                // Check specific reasons for null to return appropriate status
                var existing = await _userService.GetUserByIdAsync(userId);
                if (existing == null)
                {
                    return NotFound($"User with ID {userId} not found.");
                }
                else if (await _userService.CheckEmailExistsAsync(userDto) && existing.userEmail != userDto.userEmail)
                {
                    return Conflict("Updated email address is already in use by another user.");
                }
                else if (await _userService.CheckUsernameExistsAsync(userDto) && existing.userName != userDto.userName)
                {
                    return Conflict("Updated username is already taken by another user.");
                }
                else
                {
                    return BadRequest("Failed to update user profile due to an unknown issue.");
                }
            }
            // Return updated user details, excluding password
            return Ok(new { updatedUser.userId, updatedUser.userName, updatedUser.userEmail, updatedUser.userCreatedDate, updatedUser.userUpdatedDate });
        }

        /// <summary>
        /// Deletes a user. Requires authentication and administrative privileges.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <returns>204 No Content if deleted, or 404 Not Found.</returns>
        [HttpDelete("{userId}")]
        [Authorize(Roles = "Admin")] // Example: Only "Admin" can delete users
        public async Task<IActionResult> DeleteUser(int userId)
        {
            bool deleted = await _userService.DeleteUserAsync(userId);
            if (!deleted)
            {
                return NotFound($"User with ID {userId} not found.");
            }
            return NoContent(); // 204 No Content for successful deletion
        }

        /// <summary>
        /// Allows an authenticated user to change their password.
        /// </summary>
        /// <param name="userId">The ID of the user whose password is to be changed.</param>
        /// <param name="newPasswordDto">Object containing the new password.</param>
        /// <returns>200 OK if successful, 400 Bad Request, or 404 Not Found.</returns>
        [HttpPut("{userId}/change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int userId, [FromBody] PasswordChangeDto newPasswordDto)
        {
            if (string.IsNullOrWhiteSpace(newPasswordDto.NewPassword))
            {
                return BadRequest("New password cannot be empty.");
            }

            // Optional: Ensure the authenticated user is changing their own password
            // var authenticatedUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            // if (authenticatedUserId != userId) return Forbid();

            bool changed = await _userService.ChangePasswordAsync(userId, newPasswordDto.NewPassword);
            if (!changed)
            {
                return NotFound($"User with ID {userId} not found or password change failed.");
            }
            return Ok("Password updated successfully.");
        }

        /// <summary>
        /// Checks if a username already exists.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <returns>200 OK with true/false, or 400 Bad Request.</returns>
        [HttpGet("check-username")]
        public async Task<IActionResult> CheckUsernameExists([FromQuery] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest("Username cannot be empty.");
            }
            var user = new User { userName = username }; // Create a dummy User DTO for the service method
            bool exists = await _userService.CheckUsernameExistsAsync(user);
            return Ok(new { exists = exists });
        }

        /// <summary>
        /// Checks if an email address already exists.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>200 OK with true/false, or 400 Bad Request.</returns>
        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmailExists([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email cannot be empty.");
            }
            var user = new User { userEmail = email }; // Create a dummy User DTO for the service method
            bool exists = await _userService.CheckEmailExistsAsync(user);
            return Ok(new { exists = exists });
        }
    }

    /// <summary>
    /// DTO for password change requests.
    /// </summary>
    public class PasswordChangeDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string NewPassword { get; set; }
    }
}
