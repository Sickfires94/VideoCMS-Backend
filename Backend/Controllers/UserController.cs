using Backend.Configurations.DataConfigs;
using Backend.DTOs;
using Backend.DTOs.RequestDtos;
using Backend.DTOs.ResponseDtos;
using Backend.Services.Interface;
using Backend.Services.Mappers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserMapperService _userMapperService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, IUserMapperService userMapperService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _userMapperService = userMapperService;
            _logger = logger;
        }

       [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRequestDto request)
        {
            // Validate request
            if (!ModelState.IsValid) return BadRequest(ModelState);

            ///  TODO Handle the following using exceptions
            if (await _userService.CheckEmailExistsAsync(request.userEmail)) return Conflict("Registration failed: Email address is already registered.");
            if (await _userService.CheckUsernameExistsAsync(request.userName)) return Conflict("Registration failed: Username is already taken.");

            User user = _userMapperService.toEntity(request);
            Debug.WriteLine("User role: " + user.role);

            User registeredUser = await _userService.RegisterUserAsync(user);
            if (registeredUser == null)
            {
                    return BadRequest("Registration failed due to an unknown issue.");
            }

            UserResponseDto response = _userMapperService.toResponse(registeredUser);
            return CreatedAtAction(nameof(GetUserById), new { userId = registeredUser.userId }, response);
        }

       [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto request) // Assuming 'User' here has userEmail and userPassword
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Email and password are required.");
            }

            User user = _userMapperService.toEntity(request);

            AuthenticatedUserResponseDto authenticatedUser = await _userService.AuthenticateUserAsync(user);
            if (authenticatedUser == null)
            {
                return Unauthorized("Invalid email or password.");
            }
            
            return Ok(authenticatedUser);
        }


        [HttpGet("{userId}")]
        [Authorize] // Requires a valid JWT token
        public async Task<IActionResult> GetUserById(int userId)
        {
            if (User.Identity.IsAuthenticated && User.FindFirst(ClaimTypes.NameIdentifier)?.Value != userId.ToString()) return Forbid();

            User user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            UserResponseDto response = _userMapperService.toResponse(user);

            return Ok(response);
        }

        [HttpGet("check-username")]
        public async Task<IActionResult> CheckUsernameExists([FromQuery] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest("Username cannot be empty.");
            }
            bool exists = await _userService.CheckUsernameExistsAsync(username);
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
            bool exists = await _userService.CheckEmailExistsAsync(email);
            return Ok(new { exists = exists });
        }
    }
}
