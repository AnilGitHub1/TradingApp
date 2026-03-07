using TradingApp.Core.Entities;

namespace TradingApp.Shared.Helpers.TrendlineHelpers
{
    public class TrendlineContext
    {
        public Trendline Trendline { get; }
        public List<Candle> Candles { get; }
        public List<int> TouchIndices { get; }

        public int CurrentIndex => Candles.Count - 1;
        public Candle CurrentCandle => Candles.Last();

        public TrendlineContext(
            Trendline trendline,
            List<Candle> candles,
            List<int> touchIndices)
        {
            Trendline = trendline;
            Candles = candles;
            TouchIndices = touchIndices;
        }

        public double GetLinePriceAt(int index)
        {
            return Trendline.slope * index + Trendline.intercept;
        }
    }
}