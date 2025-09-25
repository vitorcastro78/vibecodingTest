using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupermarketAPI.API.Services;
using SupermarketAPI.Domain.Entities;
using SupermarketAPI.Infrastructure.Data;

namespace SupermarketAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SupermarketDbContext _db;
        private readonly IJwtTokenService _jwt;

        public AuthController(SupermarketDbContext db, IJwtTokenService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        public record RegisterRequest(string Email, string Password, string? WhatsAppNumber);
        public record LoginRequest(string Email, string Password);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Email e senha são obrigatórios");

            var exists = await _db.Users.AnyAsync(u => u.Email == req.Email);
            if (exists) return Conflict("Email já cadastrado");

            var user = new User
            {
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                WhatsAppNumber = req.WhatsAppNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = _jwt.GenerateToken(user.Id, user.Email);
            return Ok(new { token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (user == null) return Unauthorized();

            if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                return Unauthorized();

            var token = _jwt.GenerateToken(user.Id, user.Email);
            return Ok(new { token });
        }
    }
}


