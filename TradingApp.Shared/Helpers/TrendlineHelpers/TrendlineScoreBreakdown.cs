namespace TradingApp.Shared.Helpers.TrendlineHelpers
{
    public class TrendlineScoreBreakdown
    {
        public double TouchScore { get; set; }
        public double RecencyScore { get; set; }
        public double DistanceScore { get; set; }
        public double SpreadScore { get; set; }
        public double SlopeScore { get; set; }

        public double FinalScore { get; set; }
    }
}