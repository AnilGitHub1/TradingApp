using TradingApp.Core.DTOs;

namespace TradingApp.Core.Contracts
{
    public record FetchResult(List<Candle> Candles);
}
