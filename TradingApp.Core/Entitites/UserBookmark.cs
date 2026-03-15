namespace TradingApp.Core.Entities
{
  public class UserBookmark
  {
      public int Id { get; set; }

      public int UserId { get; set; }

      public int Token { get; set; }

      public BookmarkColor Color { get; set; }

      public DateTime CreatedAt { get; set; }

      public Users User { get; set; }
      
  }
  public enum BookmarkColor
  {
    Blue,
    Black,
    Red,
    Green,
    Yellow
  }

}
