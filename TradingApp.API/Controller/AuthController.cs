using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradingApp.Infrastructure.Data;
using TradingApp.Core.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TradingApp.Core.DTOs;
using TradingApp.Core.Interfaces;
using System.Security.Cryptography;

namespace TradingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TradingDbContext _context;
        private readonly IConfiguration _config;
        private readonly IUsersRepository _usersRepository;
        private readonly IRefreshTokenRepository _refresTokenRepository;

        public AuthController(TradingDbContext context, IUsersRepository usersRepository,
         IRefreshTokenRepository refreshTokenRepository, IConfiguration config)
        {
            _context = context;
            _config = config;
            _usersRepository = usersRepository;
            _refresTokenRepository = refreshTokenRepository;
        }

        // ================= SIGNUP =================
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] RegisterDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid request data."});

            if (string.IsNullOrWhiteSpace(dto.name))
                return BadRequest(new { message ="Name is required."});

            if (dto.name.Length < 3)
                return BadRequest(new { message ="Name must be at least 3 characters long."});


            if (string.IsNullOrWhiteSpace(dto.email))
                return BadRequest(new { message ="Email is required."});

            var gmailRegex = new System.Text.RegularExpressions.Regex(
                @"^[a-zA-Z0-9._%+-]+@gmail\.com$"
            );

            if (!gmailRegex.IsMatch(dto.email))
                return BadRequest(new { message = "Only Gmail addresses are allowed." });

            if (string.IsNullOrWhiteSpace(dto.password))
                return BadRequest(new { message ="Password is required."});

            var passwordRegex = new System.Text.RegularExpressions.Regex(
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$"
            );

            if (!passwordRegex.IsMatch(dto.password))
                return BadRequest(new { message ="Password must be at least 8 characters long and include uppercase, lowercase, number and special character."});

            if (await _context.Users.AnyAsync(x => x.email == dto.email))
                return BadRequest(new { message ="Email already exists."});

            var user = new Users(
                dto.name,
                dto.email,
                BCrypt.Net.BCrypt.HashPassword(dto.password)
            );

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message ="User created successfully."});
        }

        // ================= SIGNIN =================
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] LoginDto loginUser)
        {
          var gmailRegex = new System.Text.RegularExpressions.Regex(
                @"^[a-zA-Z0-9._%+-]+@gmail\.com$"
            );

            if (!gmailRegex.IsMatch(loginUser.email))
                return BadRequest(new { message = "Only Gmail addresses are allowed." });
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.email == loginUser.email);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            if (!BCrypt.Net.BCrypt.Verify(loginUser.password, user.password))
                return Unauthorized(new { message = "Invalid email or password" });

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenHash = HashToken(refreshToken);

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.id,
                Token = refreshTokenHash,
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",            
            };
            await _refresTokenRepository.AddRefreshTokenAsync(refreshTokenEntity);
            Response.Cookies.Append("refreshToken",refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });
            return Ok(new
            {
                access_token = token,
                user = new
                {
                    user.id,
                    user.name,
                    user.email
                }
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            var tokenHash = HashToken(refreshToken);

            var token = await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == tokenHash);

            if (token == null || !token.IsActive)
                return Unauthorized();

            // revoke old refresh token
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

            // generate new refresh token
            var newRefreshToken = GenerateRefreshToken();
            var newHash = HashToken(newRefreshToken);
                        
            token.Token = newHash;
            token.CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            token.Expires = DateTime.UtcNow.AddDays(7);
            token.ReplacedByToken = refreshToken;

            token.ReplacedByToken = refreshToken;

            await _context.SaveChangesAsync();

            // generate new access token
            var newAccessToken = GenerateJwtToken(token.User);

            Response.Cookies.Append(
                "refreshToken",
                newRefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

            return Ok(new
            {
                access_token = newAccessToken
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (refreshToken == null)
                return Ok();

            var tokenHash = HashToken(refreshToken);

            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == tokenHash);

            if (token != null)
            {
                token.Revoked = DateTime.UtcNow;
                token.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
                await _context.SaveChangesAsync();
            }

            Response.Cookies.Delete("refreshToken");

            return Ok();
        }
        private string GenerateJwtToken(Users user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Name, user.name),
                new Claim(ClaimTypes.Email, user.email)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }
        public string HashToken(string token)
        {
            using var sha = SHA256.Create();

            var hash = sha.ComputeHash(
                Encoding.UTF8.GetBytes(token)
            );

            return Convert.ToBase64String(hash);
        }
    }
}