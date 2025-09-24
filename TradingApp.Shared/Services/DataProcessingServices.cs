using Microsoft.Extensions.Logging;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure.Data;
using TradingApp.Shared.Constants;
using TradingApp.Shared.Options;

namespace TradingApp.Shared.Services
{
  public class DataProcessingService : IService
  {
    private readonly IDailyTFRepository _dailyTF;
    private readonly IFifteenTFRepository _fifteenTF;
    private readonly IHighLowRepository _highLowRepo; 
    private readonly ILogger<DataProcessingService> _logger;
    private readonly DataProcessingServiceConfig _cfg;

    public DataProcessingService(IDailyTFRepository dailyTF,
    IFifteenTFRepository fifteenTF,
    IHighLowRepository highLowRepo, RunConfig config, ILogger<DataProcessingService> logger)
    {
      _dailyTF = dailyTF;
      _fifteenTF = fifteenTF;
      _highLowRepo = highLowRepo;
      _cfg = config.DebugOptions.ProcessingConfig;
      _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
      _logger.LogInformation("DataProcessingService started with config: {@Config}", _cfg);
      var symbols = _cfg.Symbols;
      if (_cfg.Symbols.Length == 0)
      {
        symbols = AppConstants.AllTokens.Values.ToArray();
      }
      List<TimeFrame> timeFrames = GetAllTimeframes();
      foreach (var symbol in symbols)
      {
        var results = new List<HighLow>();
        if (ct.IsCancellationRequested) break;
        _logger.LogInformation("Processing data for symbol: {Symbol}", symbol);
        IList<Candle> candles = [];
        foreach (var tf in timeFrames)
        {
          _logger.LogInformation("Processing symbol: {Symbol} for timeframe: {TimeFrame}", symbol, tf);
          candles = await GetCandles(symbol, tf, candles, ct);
          var highlows = GetHighLows(candles, tf);
          if (highlows.Count > 0)
          {
            _logger.LogInformation("Found {Count} high/lows for symbol: {Symbol} in timeframe: {TimeFrame}", highlows.Count, symbol, tf);
            results.AddRange(highlows);
          }
          if (results.Count + highlows.Count > 10000)
          {
            await _highLowRepo.AddHighLowAsync(results);
            results.Clear();
          }
          else
          {
            results.AddRange(highlows);
          }
        }
        if (results.Count > 0)
          await _highLowRepo.AddHighLowAsync(results);
        _logger.LogInformation("Completed processing for symbol: {Symbol}", symbol);
      }
      _logger.LogInformation("DataProcessingService completed.");
    }

    // correct the logic for HighLow detection
    private IList<HighLow> GetHighLows(IList<Candle> candles, TimeFrame tf)
    {
      List<HighLow> highlows = new();
      for (int i = 1; i < candles.Count - 1; i++)
      {
        var prev = candles[i - 1];
        var current = candles[i];
        var next = candles[i + 1];
        if (current.high > prev.high && current.high > next.high)
        {
          highlows.Add(new HighLow(i, "h", tf.ToString(), current));
        }
        else if (current.low < prev.low && current.low < next.low)
        {
          highlows.Add(new HighLow(i, "l", tf.ToString(), current));
        }
      }
      return highlows;
    }
    private async Task<IList<Candle>> GetCandles(string symbol, TimeFrame tf, IList<Candle> existingCandles, CancellationToken ct)
    {
      TimeFrame baseTF = GetBaseTimeFrame(tf);
      if (baseTF == tf)
      {
        existingCandles = await GetCandlesFromDB(symbol, tf);
      }
      else
      {
        existingCandles = CandleConsolidator(existingCandles, tf, baseTF);
      }
      return existingCandles;
    }
    private async Task<IList<Candle>> GetCandlesFromDB(string symbol, TimeFrame tf) {
      string tokenString;
      if (!AppConstants.StockLookUP.TryGetValue(symbol, out tokenString))
      {
        throw new KeyNotFoundException();
      }
      if (!int.TryParse(tokenString, out int token))
      {
        throw new InvalidDataException();
      }
      if (tf <= TimeFrame.FourHour)
        return await _fifteenTF.GetAllFifteenTFAsync(token);
      return await _dailyTF.GetAllDailyTFAsync(token);
    }
    private IList<Candle> CandleConsolidator(IList<Candle> candles, TimeFrame requiredTF, TimeFrame sourceTF)
    {
      return candles;
    }
    private TimeFrame GetBaseTimeFrame(TimeFrame tf)
    {
      if (tf <= TimeFrame.FourHour)
        return TimeFrame.FifteenMinute;
      return TimeFrame.Day;      
    }
    private List<TimeFrame> GetAllTimeframes()
    { return new List<TimeFrame>{
        TimeFrame.FifteenMinute,
        TimeFrame.ThirtyMinute,
        TimeFrame.OneHour,
        TimeFrame.TwoHour,
        TimeFrame.FourHour,
        TimeFrame.Day,
        TimeFrame.Week,
        TimeFrame.Month
      };
    }
  }
}
