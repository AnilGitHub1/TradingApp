using TradingApp.Core.Contracts;
using TradingApp.Core.Entities;
using TradingApp.Shared.Constants;
namespace TradingApp.Core.Interfaces
{
    public interface IMarketApiClient<T> where T : Candle
    {
      Task<List<DhanScanDto>>  FetchScanDataAsync(DhanScanSettings settings, CancellationToken ct);
      /// <summary>Fetch candles for a single symbol and timeframe, entire data of the stock, returns canonical DTO.</summary>
      Task<FetchResult<T>?> FetchAsync(string symbol, TimeFrame timeFrame, CancellationToken ct);
      /// <summary>Fetch candles for symbols and timeframe, entire data of each stock, returns canonical DTO.</summary>
      Task<FetchResult<T>?> FetchAsync(List<string> symbols, TimeFrame timeFrame, CancellationToken ct);
      /// <summary>Fetch candles for all symbols and timeframe, entire data of all stocks, returns canonical DTO.</summary>
      Task<FetchResult<T>?> FetchAsync(TimeFrame timeFrame, CancellationToken ct);
      /// <summary>Fetch candles for a single symbol and timeframe, data from start, returns canonical DTO.</summary>
      Task<FetchResult<T>?> FetchAsync(string symbol, TimeFrame timeFrame, DateTime start, CancellationToken ct);
      /// <summary>Fetch candles for symbols and timeframe, data from start of each stock, returns canonical DTO.</summary>
      Task<FetchResult<T>?> FetchAsync(List<string> symbol, TimeFrame timeFrame, DateTime start, CancellationToken ct);
      /// <summary>Fetch candles for all symbols and timeframe, data from start of all stocks, returns canonical DTO.</summary>
      Task<FetchResult<T>?> FetchAsync(TimeFrame timeFrame, DateTime start, CancellationToken ct);
  }
}