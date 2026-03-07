using TradingApp.Shared.Options;
using TradingApp.Core.Entities;

namespace TradingApp.Shared.Helpers.TrendlineHelpers
{
    public class TrendlineRanker
    {
        private readonly TrendlineRankingOptions _options;

        public TrendlineRanker(TrendlineRankingOptions? options = null)
        {
            _options = options ?? new TrendlineRankingOptions();
        }

        public TrendlineScoreBreakdown Rank(TrendlineContext context)
        {
            if (!PassHardFilters(context))
                return new TrendlineScoreBreakdown { FinalScore = 0 };

            var breakdown = new TrendlineScoreBreakdown
            {
                TouchScore = CalculateTouchScore(context),
                RecencyScore = CalculateRecencyScore(context),
                DistanceScore = CalculateDistanceScore(context),
                SpreadScore = CalculateSpreadScore(context),
                SlopeScore = CalculateSlopeScore(context)
            };

            breakdown.FinalScore =
                breakdown.TouchScore * _options.TouchWeight +
                breakdown.RecencyScore * _options.RecencyWeight +
                breakdown.DistanceScore * _options.DistanceWeight +
                breakdown.SpreadScore * _options.SpreadWeight +
                breakdown.SlopeScore * _options.SlopeWeight;

            return breakdown;
        }

        private bool PassHardFilters(TrendlineContext context)
        {
            double angle = Math.Abs(Math.Atan(context.Trendline.slope) * 180 / Math.PI);

            if (angle > _options.MaxAngleDegrees)
                return false;

            double linePriceNow = context.GetLinePriceAt(context.CurrentIndex);

            // Resistance check
            if (context.Trendline.hl == "h" &&
                context.CurrentCandle.close >= linePriceNow)
                return false;

            // Support check
            if (context.Trendline.hl == "l" &&
                context.CurrentCandle.close <= linePriceNow)
                return false;

            return true;
        }

        private double CalculateTouchScore(TrendlineContext context)
        {
            int touches = context.TouchIndices.Count;
            return 1 - Math.Exp(-0.5 * touches);
        }

        private double CalculateRecencyScore(TrendlineContext context)
        {
            int gap = context.CurrentIndex - context.Trendline.index2;
            return Math.Exp(-gap / _options.RecencyDecay);
        }

        private double CalculateDistanceScore(TrendlineContext context)
        {
            double linePriceNow = context.GetLinePriceAt(context.CurrentIndex);
            double distance = Math.Abs(context.CurrentCandle.close - linePriceNow);

            double atr = CalculateATR(context.Candles, 14);

            if (atr == 0) return 0;

            double normalized = distance / atr;
            return 1 - Math.Min(normalized / _options.DistanceAtrMultiplier, 1);
        }

        private double CalculateSpreadScore(TrendlineContext context)
        {
            if (context.TouchIndices.Count < 2)
                return 0;

            var gaps = new List<int>();

            for (int i = 1; i < context.TouchIndices.Count; i++)
                gaps.Add(context.TouchIndices[i] - context.TouchIndices[i - 1]);

            double avg = gaps.Average();
            double variance = gaps.Sum(g => Math.Pow(g - avg, 2)) / gaps.Count;
            double std = Math.Sqrt(variance);

            return 1 - Math.Min(std / 100.0, 1); // 100 is tunable
        }

        private double CalculateSlopeScore(TrendlineContext context)
        {
            double angle = Math.Abs(Math.Atan(context.Trendline.slope) * 180 / Math.PI);
            return 1 - (angle / _options.MaxAngleDegrees);
        }

        private double CalculateATR(List<Candle> candles, int period)
        {
            if (candles.Count < period + 1)
                return 0;

            double sum = 0;

            for (int i = candles.Count - period; i < candles.Count; i++)
            {
                double high = candles[i].high;
                double low = candles[i].low;
                double prevClose = candles[i - 1].close;

                double tr = Math.Max(high - low,
                          Math.Max(Math.Abs(high - prevClose),
                                   Math.Abs(low - prevClose)));

                sum += tr;
            }

            return sum / period;
        }
    }
}