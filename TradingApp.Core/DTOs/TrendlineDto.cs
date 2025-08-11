namespace TradingApp.Core.DTOs
{
    public class TrendlineDto
    {
        public int Id { get; set; }
        public int Token { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public double Slope { get; set; }

        public double Intercept { get; set; }

        public string? HL { get; set; }

        public string? TF { get; set; }

        public int Index { get; set; }
        public int Index1 { get; set; }
        public int Index2 { get; set; }
        public int Connects { get; set; }
        public int TotalConnects { get; set; }
    }
}
