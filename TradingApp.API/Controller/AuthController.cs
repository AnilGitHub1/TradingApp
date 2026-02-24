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

namespace TradingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TradingDbContext _context;
        private readonly IConfiguration _config;
        private readonly IUsersRepository _usersRepository;

        public AuthController(TradingDbContext context, IUsersRepository usersRepository, IConfiguration config)
        {
            _context = context;
            _config = config;
            _usersRepository = usersRepository;
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
                expires: DateTime.Now.AddHours(4),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}