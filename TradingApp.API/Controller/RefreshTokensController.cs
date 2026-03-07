using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Core.DTOs;
using TradingApp.Core.Interfaces;

namespace TradingApp.API.Controllers
{
    [Route("api/refresh-tokens")]
    [ApiController]
    public class RefreshTokensController : ControllerBase
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _config;

        public RefreshTokensController(IRefreshTokenRepository refreshTokenRepository, IConfiguration config)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _config = config;
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAccessToken([FromBody] RefreshTokenRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required." });
            }

            var current = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
            if (current == null || current.Revoked != null || current.Expires <= DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            var newAccessToken = AuthController.GenerateJwtToken(current.UserId, _config);
            var newRefreshToken = AuthController.GenerateRefreshTokenValue();

            current.Revoked = DateTime.UtcNow;
            current.RevokedByIp = GetIpAddress();
            current.ReplacedByToken = newRefreshToken;
            await _refreshTokenRepository.UpdateRefreshTokenAsync(current);

            await _refreshTokenRepository.AddRefreshTokenAsync(new Core.Entities.RefreshToken
            {
                UserId = current.UserId,
                Token = newRefreshToken,
                Created = DateTime.UtcNow,
                CreatedByIp = GetIpAddress(),
                Expires = DateTime.UtcNow.AddDays(14)
            });

            return Ok(new
            {
                access_token = newAccessToken,
                refresh_token = newRefreshToken
            });
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] RevokeRefreshTokenRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required." });
            }

            var token = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
            if (token == null)
            {
                return NotFound(new { message = "Refresh token not found." });
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId) || token.UserId != userId)
            {
                return Forbid();
            }

            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = GetIpAddress();
            await _refreshTokenRepository.UpdateRefreshTokenAsync(token);

            return Ok(new { message = "Refresh token revoked." });
        }

        private string GetIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
