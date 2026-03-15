<<<<<<< HEAD
=======
using TradingApp.Shared.Constants;

>>>>>>> abcae4471c012cc6817891571c67a4d26bae5c70
namespace TradingApp.Core.Entities
{
  public class UserBookmark
  {
      public int Id { get; set; }

      public int UserId { get; set; }

      public int Token { get; set; }

      public BookmarkColor Color { get; set; }

      public DateTime CreatedAt { get; set; }

<<<<<<< HEAD
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
=======
      public Users? User { get; set; }
  }
}
>>>>>>> abcae4471c012cc6817891571c67a4d26bae5c70
