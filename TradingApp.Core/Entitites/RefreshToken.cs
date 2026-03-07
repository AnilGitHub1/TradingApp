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

      public Users User { get; set; }
  }
}