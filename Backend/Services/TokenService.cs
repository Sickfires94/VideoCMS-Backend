using Backend.Configurations.DataConfigs; // For JwtConfig
using Backend.DTOs;
using Backend.Services.Interface;
using Backend.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtConfig _jwtConfig;

        public TokenService(IOptions<JwtConfig> jwtConfigOptions)
        {
            _jwtConfig = jwtConfigOptions.Value;
        }

        /// <summary>
        /// Generates a JWT token for the given user, using configured settings.
        /// </summary>
        /// <param name="user">The user entity containing claims data (e.g., userId, userEmail, userName).</param>
        /// <returns>A signed JWT token string.</returns>
        public string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                // Common claims based on your User DTO
                new Claim(JwtRegisteredClaimNames.Sub, user.userId.ToString()), // Subject: User ID
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
                new Claim(JwtRegisteredClaimNames.Email, user.userEmail), // Email claim
                new Claim(ClaimTypes.Name, user.userName) // Standard Name claim, often used for principal name
                // Add additional claims here if needed, e.g., roles:
                // new Claim(ClaimTypes.Role, "Admin"),
                // new Claim(ClaimTypes.Role, "User")
            };

            // Get the signing key from configuration
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define the token's properties
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // Token valid for 1 hour. Adjust as needed.
                SigningCredentials = credentials
            };

            // Create and write the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
