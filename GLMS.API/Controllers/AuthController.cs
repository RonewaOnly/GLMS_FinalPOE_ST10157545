using GLMS.API.Data;
using GLMS.API.Models.DTOs;
using GLMS.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.API.Controllers
{
    /// <summary>Authentication — issues JWT tokens.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbAPIContext _db;
        private readonly IJwtService _jwt;
        public AuthController(ApplicationDbAPIContext db, IJwtService jwt) { _db = db; _jwt = jwt; }
        /// <summary>Login and receive a JWT. Default: admin / Admin@1234</summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user == null || req.Password != "Admin@1234")
                return Unauthorized(new { message = "Invalid username or password." });
            var token = _jwt.GenerateToken(user);
            return Ok(new LoginResponse { Token = token, Username = user.Username, Role = user.Role, Expires = DateTime.UtcNow.AddHours(8) });
        }
    }
}
