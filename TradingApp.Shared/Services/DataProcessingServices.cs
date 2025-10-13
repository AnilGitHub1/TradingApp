using Microsoft.Extensions.Logging;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
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
    private readonly List<string> _symbols;

    public DataProcessingService(IDailyTFRepository dailyTF,
    IFifteenTFRepository fifteenTF,
    IHighLowRepository highLowRepo, RunConfig config, ILogger<DataProcessingService> logger)
    {
      _dailyTF = dailyTF;
      _fifteenTF = fifteenTF;
      _highLowRepo = highLowRepo;
      _cfg = config.DebugOptions.ProcessingConfig;
      _logger = logger;
      _symbols = config.DebugOptions.Symbols;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
      _logger.LogInformation("DataProcessingService started with config: {@Config}", _cfg.ToString());
      List<TimeFrame> timeFrames = Utility.GetAllTimeframes();
      foreach (var symbol in _symbols)
      {
        var results = new SortedDictionary<DateTime, HighLow>();
        if (ct.IsCancellationRequested) break;
        _logger.LogInformation("Processing data for symbol: {Symbol}", symbol);
        IEnumerable<Candle> candles = [];
        foreach (var tf in timeFrames)
        {
          _logger.LogInformation("Processing symbol: {Symbol} for timeframe: {TimeFrame}", symbol, tf);
          candles = await GetCandles(symbol, tf, candles, ct);
          GetHighLowsForTimeFrame(candles, tf, HighLowMode.High, results);
        }
        if (results.Count > 0)
          await _highLowRepo.AddHighLowAsync(results.Values.ToList());
        _logger.LogInformation("Completed processing for symbol: {Symbol}", symbol);
      }
      _logger.LogInformation("DataProcessingService completed...");
    }
    private async Task<IEnumerable<Candle>> GetCandles(string symbol, TimeFrame tf, IEnumerable<Candle> prevCandles, CancellationToken ct)
    {
      if (ct.IsCancellationRequested) return [];
      if (tf == TimeFrame.FifteenMinute || tf == TimeFrame.Day)
      {
        return await Utility.GetCandlesFromDB(symbol, tf, _dailyTF, _fifteenTF);
      }
      else
      {
        var resampledCandles = Utility.Resample(tf, prevCandles);
        return resampledCandles;
      }
    }

    private static void GetHighLowsForTimeFrame(IEnumerable<Candle> candles, TimeFrame tf, HighLowMode mode, SortedDictionary<DateTime, HighLow> results)
    {
      string tfStr = EnumMapper.GetTimeFrame(tf);
      if (mode == HighLowMode.HighLow)
      {
        // Run both in parallel
        var highTask = Task.Run(() => ComputeHighs(candles, tfStr, results));
        var lowTask = Task.Run(() => ComputeLows(candles, tfStr, results));
        Task.WaitAll(highTask, lowTask);
      }
      else if (mode == HighLowMode.High)
      {
        ComputeHighs(candles, tfStr, results);
      }
      else if (mode == HighLowMode.Low)
      {
        ComputeLows(candles, tfStr, results);
      }
    }

    private static void ComputeHighs(IEnumerable<Candle> candles, string tf, SortedDictionary<DateTime, HighLow> results)
    {
      int n = candles.Count();
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
        {
          if (!results.TryGetValue(candles.ElementAt(i).time, out var highLow))
          {
            results[candles.ElementAt(i).time] = new HighLow("h", tf, current);
          }
          else
          {
            highLow.tf += tf;
          }
        }
      }
    }

    private static void ComputeLows(IEnumerable<Candle> candles, string tf, SortedDictionary<DateTime, HighLow> results)
    {
      int n = candles.Count();
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
        {
          if (!results.TryGetValue(candles.ElementAt(i).time, out var highLow))
          {
            results[candles.ElementAt(i).time] = new HighLow("l", tf, current);
          }
          else
          {
            highLow.tf += tf;
          }
        }
      }
    }
       
  }
}
