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
<<<<<<< HEAD
using System.Security.Cryptography;
=======
using TradingApp.Infrastructure.Data;
>>>>>>> abcae4471c012cc6817891571c67a4d26bae5c70

namespace TradingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TradingDbContext _context;
        private readonly IConfiguration _config;
<<<<<<< HEAD
        private readonly IUsersRepository _usersRepository;
        private readonly IRefreshTokenRepository _refresTokenRepository;

        public AuthController(TradingDbContext context, IUsersRepository usersRepository,
         IRefreshTokenRepository refreshTokenRepository, IConfiguration config)
        {
            _context = context;
            _config = config;
            _usersRepository = usersRepository;
            _refresTokenRepository = refreshTokenRepository;
=======
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthController(
            TradingDbContext context,
            IRefreshTokenRepository refreshTokenRepository,
            IConfiguration config)
        {
            _context = context;
            _config = config;
            _refreshTokenRepository = refreshTokenRepository;
>>>>>>> abcae4471c012cc6817891571c67a4d26bae5c70
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

<<<<<<< HEAD
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenHash = HashToken(refreshToken);
=======
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
>>>>>>> abcae4471c012cc6817891571c67a4d26bae5c70

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

<<<<<<< HEAD
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
=======
        public static string GenerateJwtToken(int userId, IConfiguration config, string userName = "", string email = "")
>>>>>>> abcae4471c012cc6817891571c67a4d26bae5c70
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
<<<<<<< HEAD
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
=======
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
>>>>>>> abcae4471c012cc6817891571c67a4d26bae5c70
        }
    }
}
