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
        public async Task<IActionResult> GetBookmarks([FromQuery] int? token)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Invalid user context." });
            }

            if (token.HasValue)
            {
                var bookmark = await _userBookmarkRepository.GetByUserAndTokenAsync(userId.Value, token.Value);
                if (bookmark == null)
                {
                    return NotFound(new { message = "Bookmark not found." });
                }

                return Ok(bookmark);
            }

            var bookmarks = await _userBookmarkRepository.GetByUserAsync(userId.Value);
            return Ok(bookmarks);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBookmarkById(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Invalid user context." });
            }

            var bookmark = await _userBookmarkRepository.GetByIdForUserAsync(id, userId.Value);
            if (bookmark == null)
            {
                return NotFound(new { message = "Bookmark not found." });
            }

            return Ok(bookmark);
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
                existing.Color = request.Color;
                await _userBookmarkRepository.UpdateUserBookmarkAsync(existing);
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
            return CreatedAtAction(nameof(GetBookmarkById), new { id = bookmark.Id }, bookmark);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditBookmark(int id, [FromBody] UpdateUserBookmarkDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Invalid user context." });
            }

            var bookmark = await _userBookmarkRepository.GetByIdForUserAsync(id, userId.Value);
            if (bookmark == null)
            {
                return NotFound(new { message = "Bookmark not found." });
            }

            bookmark.Color = request.Color;
            await _userBookmarkRepository.UpdateUserBookmarkAsync(bookmark);
            return Ok(bookmark);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBookmark(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Invalid user context." });
            }

            var bookmark = await _userBookmarkRepository.GetByIdForUserAsync(id, userId.Value);
            if (bookmark == null)
            {
                return NotFound(new { message = "Bookmark not found." });
            }

            await _userBookmarkRepository.DeleteUserBookmarkAsync(bookmark);
            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claimValue, out var userId) ? userId : null;
        }
    }
}
