using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TradingApp.Core.DTOs;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure.Data;

namespace TradingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TradingDbContext _context;
        private readonly IConfiguration _config;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthController(
            TradingDbContext context,
            IRefreshTokenRepository refreshTokenRepository,
            IConfiguration config)
        {
            _context = context;
            _config = config;
            _refreshTokenRepository = refreshTokenRepository;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] RegisterDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid request data." });

            if (string.IsNullOrWhiteSpace(dto.name) || dto.name.Length < 3)
                return BadRequest(new { message = "Name must be at least 3 characters long." });

            if (string.IsNullOrWhiteSpace(dto.email))
                return BadRequest(new { message = "Email is required." });

            var gmailRegex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9._%+-]+@gmail\.com$");
            if (!gmailRegex.IsMatch(dto.email))
                return BadRequest(new { message = "Only Gmail addresses are allowed." });

            if (string.IsNullOrWhiteSpace(dto.password))
                return BadRequest(new { message = "Password is required." });

            var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$");
            if (!passwordRegex.IsMatch(dto.password))
                return BadRequest(new { message = "Password must be at least 8 characters long and include uppercase, lowercase, number and special character." });

            if (await _context.Users.AnyAsync(x => x.email == dto.email))
                return BadRequest(new { message = "Email already exists." });

            var user = new Users(dto.name, dto.email, BCrypt.Net.BCrypt.HashPassword(dto.password));
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User created successfully." });
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] LoginDto loginUser)
        {
            var gmailRegex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9._%+-]+@gmail\.com$");
            if (!gmailRegex.IsMatch(loginUser.email))
                return BadRequest(new { message = "Only Gmail addresses are allowed." });

            var user = await _context.Users.FirstOrDefaultAsync(x => x.email == loginUser.email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginUser.password, user.password))
                return Unauthorized(new { message = "Invalid email or password" });

            var accessToken = GenerateJwtToken(user.id, _config, user.name, user.email);
            var refreshTokenValue = GenerateRefreshTokenValue();
            var existing = await _refreshTokenRepository.GetActiveByUserIdAsync(user.id);

            if (existing != null)
            {
                existing.Revoked = DateTime.UtcNow;
                existing.RevokedByIp = GetIpAddress();
                existing.ReplacedByToken = refreshTokenValue;
                await _refreshTokenRepository.UpdateRefreshTokenAsync(existing);
            }

            await _refreshTokenRepository.AddRefreshTokenAsync(new RefreshToken
            {
                UserId = user.id,
                Token = refreshTokenValue,
                Created = DateTime.UtcNow,
                CreatedByIp = GetIpAddress(),
                Expires = DateTime.UtcNow.AddDays(14)
            });

            return Ok(new
            {
                access_token = accessToken,
                refresh_token = refreshTokenValue,
                user = new
                {
                    user.id,
                    user.name,
                    user.email
                }
            });
        }

        public static string GenerateJwtToken(int userId, IConfiguration config, string userName = "", string email = "")
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString())
            };

            if (!string.IsNullOrWhiteSpace(userName)) claims.Add(new Claim(ClaimTypes.Name, userName));
            if (!string.IsNullOrWhiteSpace(email)) claims.Add(new Claim(ClaimTypes.Email, email));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string GenerateRefreshTokenValue()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }

        private string GetIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
