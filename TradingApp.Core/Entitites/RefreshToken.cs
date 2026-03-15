namespace TradingApp.Core.Entities
{
  public class RefreshToken
  {
      public int Id { get; set; }

      public int UserId { get; set; }

      public string Token { get; set; } = "";

      public DateTime Expires { get; set; }

      public DateTime Created { get; set; }

      public string CreatedByIp { get; set; } = "";

      public DateTime? Revoked { get; set; }

      public string RevokedByIp { get; set; } = "";

      public string ReplacedByToken { get; set; } = "";
      public bool IsExpired => DateTime.UtcNow >= Expires;

      public bool IsActive => Revoked == null && !IsExpired;
      public static RefreshToken EmptyRefreshToken()
      {
        return new RefreshToken();
      }
      public static bool IsValidRefreshToken(RefreshToken rt)
      {
        if(rt.Token == "") return false;
        return true;
      }
      public Users User { get; set; }
  }
}