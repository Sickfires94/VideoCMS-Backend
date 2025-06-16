using Backend.Services;
using Backend.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UserController : ControllerBase
    {

        private readonly UserService _userService;
        private readonly ILogger _logger;

        public UserController(ILogger<UserController> logger ,UserService userService) {
            _userService = userService;
            _logger = logger;

        }


        [HttpGet]

        public IEnumerable<User> Get()
        {
            _logger.LogInformation("Retrieving all users");
            return _userService.GetAllUsers();
        }

        [HttpPost]
        public User Add([FromBody] User user)
        {
            _logger.LogInformation("Adding user");
            _userService.AddUser(user);
            return user;
        }
    }
}
