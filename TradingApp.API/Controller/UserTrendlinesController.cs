using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Core.DTOs;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;

namespace TradingApp.API.Controllers
{
    [Route("api/user-trendlines")]
    [ApiController]
    [Authorize]
    public class UserTrendlinesController : ControllerBase
    {
        private readonly IUserTrendlineRepository _userTrendlineRepository;

        public UserTrendlinesController(IUserTrendlineRepository userTrendlineRepository)
        {
            _userTrendlineRepository = userTrendlineRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrendlines([FromQuery] int? token, [FromQuery] string? tf)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Invalid user context." });
            }

            IList<UserTrendline> trendlines;
            if (token.HasValue)
            {
                trendlines = await _userTrendlineRepository.GetByUserAndTokenAsync(userId.Value, token.Value, tf);
            }
            else
            {
                trendlines = await _userTrendlineRepository.GetByUserAsync(userId.Value);
                if (!string.IsNullOrWhiteSpace(tf))
                {
                    trendlines = trendlines.Where(x => x.Tf == tf).ToList();
                }
            }

            return Ok(trendlines);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetTrendlineById(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Invalid user context." });
            }

            var trendline = await _userTrendlineRepository.GetByIdForUserAsync(id, userId.Value);
            if (trendline == null)
            {
                return NotFound(new { message = "Trendline not found." });
            }

            return Ok(trendline);
        }

        [HttpPost]
        public async Task<IActionResult> SaveTrendline([FromBody] SaveUserTrendlineDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Invalid user context." });
            }

            if (request.Token <= 0 || string.IsNullOrWhiteSpace(request.Tf))
            {
                return BadRequest(new { message = "Token and timeframe are required." });
            }

            var trendline = new UserTrendline
            {
                UserId = userId.Value,
                Token = request.Token,
                Tf = request.Tf,
                StartValue = request.StartValue,
                StartTime = request.StartTime,
                EndValue = request.EndValue,
                EndTime = request.EndTime,
                Slope = request.Slope,
                Intercept = request.Intercept,
                Index1 = request.Index1,
                Index2 = request.Index2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userTrendlineRepository.AddUserTrendlineAsync(trendline);
            return CreatedAtAction(nameof(GetTrendlineById), new { id = trendline.Id }, trendline);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditTrendline(int id, [FromBody] UpdateUserTrendlineDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Invalid user context." });
            }

            var trendline = await _userTrendlineRepository.GetByIdForUserAsync(id, userId.Value);
            if (trendline == null)
            {
                return NotFound(new { message = "Trendline not found." });
            }

            if (!string.IsNullOrWhiteSpace(request.Tf)) trendline.Tf = request.Tf;
            if (request.StartValue.HasValue) trendline.StartValue = request.StartValue.Value;
            if (request.StartTime.HasValue) trendline.StartTime = request.StartTime.Value;
            if (request.EndValue.HasValue) trendline.EndValue = request.EndValue.Value;
            if (request.EndTime.HasValue) trendline.EndTime = request.EndTime.Value;
            if (request.Slope.HasValue) trendline.Slope = request.Slope.Value;
            if (request.Intercept.HasValue) trendline.Intercept = request.Intercept.Value;
            if (request.Index1.HasValue) trendline.Index1 = request.Index1.Value;
            if (request.Index2.HasValue) trendline.Index2 = request.Index2.Value;
            trendline.UpdatedAt = DateTime.UtcNow;

            await _userTrendlineRepository.UpdateUserTrendlineAsync(trendline);
            return Ok(trendline);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTrendline(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Invalid user context." });
            }

            var trendline = await _userTrendlineRepository.GetByIdForUserAsync(id, userId.Value);
            if (trendline == null)
            {
                return NotFound(new { message = "Trendline not found." });
            }

            await _userTrendlineRepository.DeleteUserTrendlineAsync(trendline);
            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claimValue, out var userId) ? userId : null;
        }
    }
}
