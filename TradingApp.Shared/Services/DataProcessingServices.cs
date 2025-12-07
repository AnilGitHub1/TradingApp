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
      _logger.LogInformation("highlow data ProcessingService started with config: {@Config}", _cfg.ToString());
      List<TimeFrame> timeFrames = Utility.GetAllTimeframes();
      foreach (var symbol in _symbols)
      {
        var results = new SortedDictionary<DateTime, HighLow>();
        Utility.GetToken(symbol, out int token);
        if(token == -1)
        {
          _logger.LogWarning("Token not found for symbol: {Symbol}, skipping...", symbol);
          continue;
        }
        if (ct.IsCancellationRequested) break;
        _logger.LogInformation("highlow data Processing for symbol: {Symbol}", symbol);
        IEnumerable<Candle> candles = new List<Candle>();
        var startTime = DateTime.MinValue;
        foreach (var tf in timeFrames)
        {
          var analysisStartTime = await GetProcessingStartTime(token, tf);
          candles = await Utility.GetCandles(token, tf, candles?? new List<Candle>(), _dailyTF, _fifteenTF, analysisStartTime);
          if(candles == null || candles.Count() < 6)
          {
            _logger.LogInformation("No candles found for symbol: {Symbol}, timeframe: {TimeFrame}", symbol, tf);
            continue;
          }
          var analysisCandles = new List<Candle>(candles.Count());
          if (analysisStartTime == default)
          {
            analysisCandles = candles.ToList();
          }
          else
          {
            bool startFound = false;
            for (int i = 0; i < candles.Count(); i++)
            {
              if (startFound || ( i+Window < candles.Count() && candles.ElementAt(i + Window).time > analysisStartTime))
              {
                startFound = true;
                analysisCandles.Add(candles.ElementAt(i));
              }
            }
          }
          GetHighLowsForTimeFrame(analysisCandles, tf, HighLowType.High, results);
        }
        if (results.Count > 0)
          await _highLowRepo.AddHighLowAsync(results.Values.ToList());
        _logger.LogInformation("Completed highlow data processing for symbol: {Symbol}", symbol);
      }
      _logger.LogInformation("DataProcessingService completed...");
    }    
    private static void GetHighLowsForTimeFrame(IEnumerable<Candle> candles, TimeFrame tf, HighLowType mode, SortedDictionary<DateTime, HighLow> results)
    {
      string tfStr = EnumMapper.GetTimeFrame(tf);
      if (mode == HighLowType.HighLow)
      {
        // Run both in parallel
        var highTask = Task.Run(() => ComputeHighs(candles, tfStr, results));
        var lowTask = Task.Run(() => ComputeLows(candles, tfStr, results));
        Task.WaitAll(highTask, lowTask);
      }
      else if (mode == HighLowType.High)
      {
        ComputeHighs(candles, tfStr, results);
      }
      else if (mode == HighLowType.Low)
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
          if (!results.TryGetValue(current.time, out var highLow))
          {
            results[current.time] = new HighLow("h", tf, current.token, current.time);
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
            results[candles.ElementAt(i).time] = new HighLow("l", tf, current.token, current.time);
          }
          else
          {
            highLow.tf += tf;
          }
        }
      }
    }
    private async Task<DateTime> GetProcessingStartTime(int token, TimeFrame tf)
    {
      var lastestHighLow = await _highLowRepo.GetLatestHighLowAsync(token, EnumMapper.GetTimeFrame(tf));
      if (lastestHighLow != null)
      {
        return lastestHighLow.time;
      }
      else
      {
        return default;
      }
    }
  }
}
