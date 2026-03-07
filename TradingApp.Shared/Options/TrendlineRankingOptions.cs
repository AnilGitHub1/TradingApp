namespace TradingApp.Shared.Options
{
    public class TrendlineRankingOptions
    {
        public double TouchWeight { get; set; } = 0.35;
        public double RecencyWeight { get; set; } = 0.25;
        public double DistanceWeight { get; set; } = 0.20;
        public double SpreadWeight { get; set; } = 0.10;
        public double SlopeWeight { get; set; } = 0.10;

        public double MaxAngleDegrees { get; set; } = 45;
        public double RecencyDecay { get; set; } = 50;
        public double DistanceAtrMultiplier { get; set; } = 2;
    }
}