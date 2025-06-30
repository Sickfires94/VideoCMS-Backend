using Backend.Services.Interfaces;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Backend.Services
{
    public class TokenClaimsAccessor : ITokenClaimsAccessor
    {
        private readonly ILogger<ITokenClaimsAccessor> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenClaimsAccessor(IHttpContextAccessor httpContextAccessor, ILogger<ITokenClaimsAccessor> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public string? getLoggedInUserEmail()
        {
            // returns email if logged in, null otherwise
            return _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Email);
        }

        public int? getLoggedInUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if(userId == null) return Int32.Parse(userId);
            
            // return null if user is not logged in
            return null;
        }

        public string? getLoggedInUserName()
        {
            // returns userName if logged in, null otherwise
            return _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Name);
        }
    }
}
