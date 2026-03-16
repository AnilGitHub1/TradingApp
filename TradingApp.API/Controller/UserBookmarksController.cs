using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Core.DTOs;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;

namespace TradingApp.API.Controllers
{
    [Route("api/user-bookmarks")]
    [ApiController]
    [Authorize]
    public class UserBookmarksController : ControllerBase
    {
        private readonly IUserBookmarkRepository _userBookmarkRepository;

        public UserBookmarksController(IUserBookmarkRepository userBookmarkRepository)
        {
            _userBookmarkRepository = userBookmarkRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetBookmarks()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Invalid user context." });
            }

            var bookmarks = await _userBookmarkRepository.GetByUserAsync(userId.Value);
            return Ok(bookmarks);
        }
        [HttpPost]
        public async Task<IActionResult> SaveBookmark([FromBody] SaveUserBookmarkDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Invalid user context." });
            }

            if (request.Token <= 0)
            {
                return BadRequest(new { message = "Token is required." });
            }

            var existing = await _userBookmarkRepository.GetByUserAndTokenAsync(userId.Value, request.Token);
            if (existing != null)
            {
              if(existing.Color == request.Color)
                await _userBookmarkRepository.DeleteUserBookmarkAsync(existing);
              else {
                existing.Color = request.Color;
                await _userBookmarkRepository.UpdateUserBookmarkAsync(existing);
              }
              return Ok(existing);
            }

            var bookmark = new UserBookmark
            {
                UserId = userId.Value,
                Token = request.Token,
                Color = request.Color,
                CreatedAt = DateTime.UtcNow
            };

            await _userBookmarkRepository.AddUserBookmarkAsync(bookmark);
            return Ok(bookmark);
        }
        private int? GetCurrentUserId()
        {
            var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claimValue, out var userId) ? userId : null;
        }
    }
}
