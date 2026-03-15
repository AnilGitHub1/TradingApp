using TradingApp.Core.Entities;
namespace TradingApp.Core.DTOs
{
    public class SaveUserBookmarkDto
    {
        public int Token { get; set; }
        public BookmarkColor Color { get; set; }
    }

    public class UpdateUserBookmarkDto
    {
        public BookmarkColor Color { get; set; }
    }
}
