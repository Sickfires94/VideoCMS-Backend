using Backend.Configurations.DataConfigs; // For JwtConfig
using Backend.Services.Interfaces; // For ITokenService
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;
using Backend.DTOs; // For List<Claim>

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
        /// <param name="user">The user entity containing claims data (e.g., userId, userEmail, userName, Role).</param>
        /// <returns>A signed JWT token string.</returns>
        public string GenerateJwtToken(User user) // Parameter changed from Backend.DTOs.User to Backend.Models.User
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.userId.ToString()), // Subject: User ID
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
                new Claim(JwtRegisteredClaimNames.Email, user.userEmail), // Email claim
                new Claim(ClaimTypes.Name, user.userName) // Standard Name claim, often used for principal name
            };

            // ADDING ROLE CLAIM (CRITICAL for authorization)
            if (!string.IsNullOrEmpty(user.role))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.role));
            }

            // Get the signing key from configuration
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define the token's properties
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1), // Use ExpiryMinutes from JwtConfig
                SigningCredentials = credentials,
                // Removed Issuer and Audience from token generation to match validation:
                // Issuer = _jwtConfig.Issuer,
                // Audience = _jwtConfig.Audience
            };

            // Create and write the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}