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
    private const int Window = 5;
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
        IEnumerable<Candle> candles = [];
        TimeFrame prevTF = TimeFrame.FifteenMinute;
        foreach (var tf in timeFrames)
        {
          _logger.LogInformation("Processing symbol: {Symbol} for timeframe: {TimeFrame}", symbol, tf);
          candles = await GetCandles(symbol, tf, prevTF, candles, ct);
          var highlows = GetHighLows(candles, tf, HighLowMode.High);
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
          prevTF = tf;
        }
        if (results.Count > 0)
          await _highLowRepo.AddHighLowAsync(results);
        _logger.LogInformation("Completed processing for symbol: {Symbol}", symbol);
      }
      _logger.LogInformation("DataProcessingService completed.");
    }

    private static List<HighLow> GetHighLows(IEnumerable<Candle> candles, TimeFrame tf, HighLowMode mode)
    {
      int n = candles.Count();
      var results = new List<HighLow>();

      List<HighLow>? highs = null;
      List<HighLow>? lows = null;

      if (mode == HighLowMode.HighLow)
      {
        // Run both in parallel
        var highTask = Task.Run(() => ComputeHighs(candles, tf));
        var lowTask = Task.Run(() => ComputeLows(candles, tf));
        Task.WaitAll(highTask, lowTask);

        highs = highTask.Result;
        lows = lowTask.Result;
        results.AddRange(highs);
        results.AddRange(lows);
      }
      else if (mode == HighLowMode.High)
      {
          results = ComputeHighs(candles, tf);
      }
      else if (mode == HighLowMode.Low)
      {
          results = ComputeLows(candles, tf);
      }

      return results;
    }

    private static List<HighLow> ComputeHighs(IEnumerable<Candle> candles, TimeFrame tf)
    {
      int n = candles.Count();
      var highs = new List<HighLow>();
      for (int i = Window; i < n - Window; i++)
      {
        var current = candles.ElementAt(i);
        double curHigh = current.high;
        bool isLocalHigh = true;

        for (int j = i - Window; j <= i + Window; j++)
        {
          if (candles.ElementAt(j).high > curHigh)
          {
            isLocalHigh = false;
            break;
          }
        }

        if (isLocalHigh)
          highs.Add(new HighLow(i, "h", tf.ToString(), current));
      }
      return highs;
    }

    private static List<HighLow> ComputeLows(IEnumerable<Candle> candles, TimeFrame tf)
    {
      int n = candles.Count();
      var lows = new List<HighLow>();
      for (int i = Window; i < n - Window; i++)
      {
        var current = candles.ElementAt(i);
        double curLow = current.low;
        bool isLocalLow = true;

        for (int j = i - Window; j <= i + Window; j++)
        {
          if (candles.ElementAt(j).low < curLow)
          {
            isLocalLow = false;
            break;
          }
        }

        if (isLocalLow)
          lows.Add(new HighLow(i, "l", tf.ToString(), current));
      }
      return lows;
    }

    private async Task<IEnumerable<Candle>> GetCandles(string symbol, TimeFrame requiredTF, TimeFrame sourceTF, IEnumerable<Candle> existingCandles, CancellationToken ct)
    {
      TimeFrame baseTF = GetBaseTimeFrame(sourceTF);
      if (baseTF == sourceTF)
      {
        existingCandles = await GetCandlesFromDB(symbol, baseTF);
      }
      else
      {
        existingCandles = CandleConsolidator(existingCandles, requiredTF, sourceTF);
      }
      return existingCandles;
    }
    private async Task<IEnumerable<Candle>> GetCandlesFromDB(string symbol, TimeFrame tf) {
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
    private IEnumerable<Candle> CandleConsolidator(IEnumerable<Candle> candles, TimeFrame requiredTF, TimeFrame sourceTF)
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
