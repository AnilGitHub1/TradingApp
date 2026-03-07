namespace TradingApp.Core.Entities
{
    public class TrendlineScore
    {
        public int Id { get; set; }   // same as Trendline.id

        public double TouchScore { get; set; }
        public double RecencyScore { get; set; }
        public double DistanceScore { get; set; }
        public double SpreadScore { get; set; }
        public double SlopeScore { get; set; }

        public double FinalScore { get; set; }

        public string ScoreVersion { get; set; } = "v1";

        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property (important)
        public Trendline Trendline { get; set; } = null!;
    }
}