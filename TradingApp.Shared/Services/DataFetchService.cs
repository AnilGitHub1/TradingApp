using Microsoft.Extensions.Logging;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Shared.Constants;
using TradingApp.Shared.ExternalApis;
using TradingApp.Shared.Options;

namespace TradingApp.Shared.Services
{
  public class DataFetchService<T> : IService where T : Candle
  {
    private readonly ICandleRepository<T> _repo;
    private readonly ILogger<DataFetchService<T>> _logger;
    private readonly FetchServiceConfig _cfg;
    private readonly List<string> _symbols;
    private readonly IMarketApiClient<T> _client;

    public DataFetchService(ICandleRepository<T> repo, IMarketApiFactory<T> factory, ILogger<DataFetchService<T>> logger, RunConfig cfg)
    {
      _logger = logger;
      _cfg = cfg.DebugOptions.FetchConfig;
      _symbols = cfg.DebugOptions.Symbols;
      _client = factory.GetClient(EnumMapper.GetClient(_cfg.Client));
      _repo = repo;
    }
    public async Task ExecuteAsync(CancellationToken ct)
    {
      _logger.LogInformation("Starting data fetch service for {timeFrame} candles using Dhan client.", _cfg.TimeFrame);
      if (_client == null)
      {
        _logger.LogError("No client registered for Dhan provider.");
      }
      await UpdateCandlesAsync(_cfg.TimeFrame, ct);
    }
    // get all candles for a symbol from default start date based on timeframe
    // and Inserts into Db In bulk
    private async Task FetchAllCandlesAsync(string symbol, TimeFrame timeFrame, CancellationToken ct)
    {
      var result = await _client.FetchAsync(symbol, timeFrame, ct);
      if (result == null || result.Candles == null)
      {
        _logger.LogInformation("No candles fetched for {symbol}.", symbol);
        return;
      }
      var candles = result.Candles;
      if (candles.Count > 0)
      {
        _logger.LogInformation("Fetched {count} candles for {symbol}. Inserting into database.", candles.Count, symbol);
        await Insert(candles);
      }
      else
      {
        _logger.LogInformation("No candles fetched for {symbol}.", symbol);
      }
    }
    private async Task UpdateCandlesAsync(TimeFrame timeFrame, CancellationToken ct)
    {
      var combinedResult = new List<T>();
      foreach (var symbol in _symbols)
      {
        var start = await GetLatestDateTime(symbol);
        if (start == default)
        {
          _logger.LogInformation("No existing data for {symbol}. Fetching all candles.", symbol);
          await FetchAllCandlesAsync(symbol, timeFrame, ct);
          continue;
        }
        continue;
        var result = await _client.FetchAsync(symbol, timeFrame, start, ct);
        if (result == null || result.Candles == null)
        {
          continue;
        }
        var candles = result.Candles;
        if (candles.Count == 0)
        {
          _logger.LogInformation("No new candles to update for {symbol}.", symbol);
          continue;
        }
        _logger.LogInformation("Fetched {count} new candles for {symbol}.", candles.Count, symbol);
        if (combinedResult.Count + candles.Count > 10000)
        {
          await Insert(combinedResult);
          combinedResult.Clear();
          combinedResult.AddRange(candles);
        }
        else
          combinedResult.AddRange(candles);
      }
      if (combinedResult.Count > 0)
        await Insert(combinedResult);
    }
    private async Task Insert(List<T> candles)
    {
      await _repo.AddAsync(candles);
    }

    private async Task<DateTime> GetLatestDateTime(string symbol)
    {
      if (!AppConstants.StockLookUP.TryGetValue(symbol, out var tokenString))
      {
        _logger.LogWarning("Symbol {symbol} not found in token list.", symbol);
        return default;
      } 
      if (!int.TryParse(tokenString, out var token))
      {
        _logger.LogWarning("Token {tokenString} for symbol {symbol} is not a valid integer.", tokenString, symbol);
        return default;
      }
      return await _repo.GetLatestDateTimeAsync(token);
    }
    
  }
}
