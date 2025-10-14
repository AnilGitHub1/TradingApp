using TradingApp.Core.Entities;

namespace TradingApp.Core.Contracts
{
  public record FetchResult<T>(List<T> Candles) where T : Candle;
  public record AnalysisResult(List<Trendline> Trendlines);
}
