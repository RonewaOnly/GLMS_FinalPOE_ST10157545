using GLMS.API.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace GLMS.API.Services
{

        public interface IJwtService { string GenerateToken(AppUser user); }

        public class JwtService : IJwtService
        {
            private readonly IConfiguration _config;
            public JwtService(IConfiguration config) => _config = config;

            public string GenerateToken(AppUser user)
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiry = DateTime.UtcNow.AddHours(8);
                var claims = new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name,           user.Username),
                new Claim(ClaimTypes.Role,           user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
                var token = new JwtSecurityToken(issuer: _config["Jwt:Issuer"], audience: _config["Jwt:Audience"],
                    claims: claims, expires: expiry, signingCredentials: creds);
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
        }
    

}
