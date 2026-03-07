namespace TradingApp.Core.Entities
{
  public class UserTrendline
  {
      public int Id { get; set; }

      public int UserId { get; set; }

      public int Token { get; set; }

      public string Tf { get; set; }

      public decimal StartValue { get; set; }

      public DateTime StartTime { get; set; }

      public decimal EndValue { get; set; }

      public DateTime EndTime { get; set; }

      public decimal Slope { get; set; }

      public decimal Intercept { get; set; }

      public int? Index1 { get; set; }

      public int? Index2 { get; set; }

      public DateTime CreatedAt { get; set; }

      public DateTime UpdatedAt { get; set; }

      public Users User { get; set; }
  }
}