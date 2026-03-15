namespace TradingApp.Core.DTOs
{
    public class SaveUserTrendlineDto
    {
        public int Token { get; set; }
        public string Tf { get; set; } = "";
        public decimal StartValue { get; set; }
        public DateTime StartTime { get; set; }
        public decimal EndValue { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Slope { get; set; }
        public decimal Intercept { get; set; }
        public int? Index1 { get; set; }
        public int? Index2 { get; set; }
    }

    public class UpdateUserTrendlineDto
    {
        public string? Tf { get; set; }
        public decimal? StartValue { get; set; }
        public DateTime? StartTime { get; set; }
        public decimal? EndValue { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? Slope { get; set; }
        public decimal? Intercept { get; set; }
        public int? Index1 { get; set; }
        public int? Index2 { get; set; }
    }
}
