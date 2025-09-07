using TradingApp.Core.Contracts;
namespace TradingApp.Core.Interfaces
{
    public interface IMarketApiClient
    {
        /// <summary>Fetch candles for a single symbol and timeframe, entire data of the stock, returns canonical DTO.</summary>
        Task<FetchResult?> FetchAsync(string symbol, string timeframe, CancellationToken ct);
        /// <summary>Fetch candles for symbols and timeframe, entire data of each stock, returns canonical DTO.</summary>
        Task<FetchResult?> FetchAsync(List<string> symbols, string timeframe, CancellationToken ct);
        /// <summary>Fetch candles for all symbols and timeframe, entire data of all stocks, returns canonical DTO.</summary>
        Task<FetchResult?> FetchAsync(string timeframe, CancellationToken ct);
        /// <summary>Fetch candles for a single symbol and timeframe, data from start, returns canonical DTO.</summary>
        Task<FetchResult?> FetchAsync(string symbol, string timeframe, DateTime start, CancellationToken ct);
        /// <summary>Fetch candles for symbols and timeframe, data from start of each stock, returns canonical DTO.</summary>
        Task<FetchResult?> FetchAsync(List<string> symbol, string timeframe, DateTime start, CancellationToken ct);
        /// <summary>Fetch candles for all symbols and timeframe, data from start of all stocks, returns canonical DTO.</summary>
        Task<FetchResult?> FetchAsync(string timeframe, DateTime start, CancellationToken ct);
    }
}