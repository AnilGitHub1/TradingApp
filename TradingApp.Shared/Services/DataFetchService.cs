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

    public DataFetchService(ICandleRepository<DailyTF> dailyTF,
    ICandleRepository<FifteenTF> fifteenTF, IMarketApiFactory<T> factory, ILogger<DataFetchService<T>> logger, RunConfig cfg)
    {
      _logger = logger;
      _cfg = cfg.DebugOptions.FetchConfig;
      _symbols = cfg.DebugOptions.Symbols.Count() == 0 ?
      [.. AppConstants.AllTokens.Values] :
      cfg.DebugOptions.Symbols;
      _client = factory.GetClient(EnumMapper.GetClient(_cfg.client));
      _repo = GetRepository<T>(_cfg.timeFrame,dailyTF,fifteenTF);
    }
    public async Task ExecuteAsync(CancellationToken ct)
    {
      if (_client == null)
      {
        _logger.LogError("No client registered for Dhan provider.");
      }
      else
      {
        await UpdateLatestCandlesAsync(_cfg.timeFrame, ct);
      }
    }
    private async Task UpdateLatestCandlesAsync(TimeFrame timeFrame, CancellationToken ct)
    {
      string tf = EnumMapper.GetTimeFrame(timeFrame);
      var combinedResult = new List<T>();
      foreach (var symbol in _symbols)
      {
        var start = await GetLatestDateTime(438);
        var result = await _client.FetchAsync(symbol, tf, start, ct);
        if (result == null || result.Candles == null)
        {
          continue;
        }
        var candles = result.Candles;
        if (start == default)
          await Insert(candles);
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

    private async Task<DateTime> GetLatestDateTime(int token)
    {
      return await _repo.GetLatestDateTimeAsync(token);
    }
    private ICandleRepository<T> GetRepository<T>(TimeFrame timeFrame,
    ICandleRepository<DailyTF> dailyTF, ICandleRepository<FifteenTF> fifteenTF)
        where T : Candle
    {
      return timeFrame switch
      {
        TimeFrame.Day => (ICandleRepository<T>) dailyTF,
        TimeFrame.FifteenMinute => (ICandleRepository<T>) fifteenTF,
        _ => throw new NotSupportedException()
      };
    }
  }
}
