using Microsoft.Extensions.Logging;
using TradingApp.Infrastructure.Data;
using TradingApp.Core.Contracts;
using TradingApp.Core.Interfaces;
using TradingApp.Shared.Options;
using TradingApp.Shared.Constants;
using TradingApp.Core.Entities;
using System.Collections.Concurrent;

namespace TradingApp.Shared.Services
{

  public class AnalysisService : IService
  {
    private readonly IDailyTFRepository _dailyTF;
    private readonly IFifteenTFRepository _fifteenTF;
    private readonly AnalysisServiceConfig _cfg;
    private readonly List<string> _symbols;
    private readonly IHighLowRepository _highLowRepo;
    private readonly ITrendlineRepository _trendlineRepo;
    private readonly ILogger<AnalysisService> _logger;

    public AnalysisService(IDailyTFRepository dailyTF, IFifteenTFRepository fifteenTF,
    RunConfig cfg, ITrendlineRepository trendlineRepo, IHighLowRepository highLowRepo, ILogger<AnalysisService> logger)
    {
      _dailyTF = dailyTF;
      _fifteenTF = fifteenTF;
      _cfg = cfg.DebugOptions.AnalysisConfig;
      _symbols = cfg.DebugOptions.Symbols;
      _highLowRepo = highLowRepo;
      _trendlineRepo = trendlineRepo;
      _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
      _logger.LogInformation("AnalysisService started with config: {@Config}", _cfg.ToString());
      List<TimeFrame> timeFrames = Utility.GetAllTimeframes();
      var results = new AnalysisResult(new List<Trendline>());
      foreach (var symbol in _symbols)
      {
        if (ct.IsCancellationRequested) break;
        _logger.LogInformation("Processing data for symbol: {Symbol}", symbol);
        IEnumerable<Candle> candles = [];
        IList<HighLow> highLows = await GetHighLows(symbol);
        foreach (var tf in timeFrames)
        {
          _logger.LogInformation("Processing symbol: {Symbol} for timeframe: {TimeFrame}", symbol, tf);
          candles = await GetCandles(symbol, tf, candles, ct);
          var highLowCandles = GetHighLowCandles(candles, highLows, tf, HighLowType.High);
          var highsForDownTrendLines = GetHighsForDownTrendLines(highLowCandles, candles, HighLowType.High);
          GetTrendLinesForTimeFrame(candles, highsForDownTrendLines, tf);
        }
        _logger.LogInformation("Completed processing for symbol: {Symbol}", symbol);
      }
      if (results.Trendlines.Count > 0)
        await _trendlineRepo.AddTrendlineAsync(results.Trendlines.ToList());
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
    private async Task<IList<HighLow>> GetHighLows(string symbol)
    {
      string tokenString;
      if (!AppConstants.StockLookUP.TryGetValue(symbol, out tokenString))
      {
        throw new KeyNotFoundException();
      }
      if (!int.TryParse(tokenString, out int token))
      {
        throw new InvalidDataException();
      }
      return await _highLowRepo.GetAllHighLowAsync(token);
    }
    public IList<int> GetHighLowCandles(IEnumerable<Candle> candles, IList<HighLow> highLows, TimeFrame tf, HighLowType hl)
    {
      if (candles == null || !candles.Any() || highLows == null || !highLows.Any())
        return new List<int>();

      var highLowCandles = new List<int>();
      string hlStr = hl == HighLowType.High ? "h" : "l";
      string tfStr = EnumMapper.GetTimeFrame(tf);
      var highLowSet = new HashSet<DateTime>(highLows.Where(x => x.hl.Contains(hlStr))
      .Where(x => x.tf.Contains(tfStr))
      .Select(x => x.time));
      int index = 0;
      foreach (var candle in candles)
      {
        if (highLowSet.Contains(candle.time))
        {
          highLowCandles.Add(index);
        }
        index++;
      }

      return highLowCandles;
    }
    public IList<int> GetHighsForDownTrendLines(IList<int> highs, IEnumerable<Candle> candles, HighLowType hl)
    {
      if (highs == null || highs.Count == 0)
        return new List<int>();

      var result = new List<int>
      {
        highs[^1] // start from the last high
      };

      // iterate backwards except last element
      for (int i = highs.Count - 2; i >= 0; i--)
      {
        var currentHigh = candles.ElementAt(highs[i]);
        var lastAcceptedHigh = candles.ElementAt(result[^1]);

        // Assuming CompareCandles is a static method returning CandleComparer
        if (Utility.CompareCandles(currentHigh, lastAcceptedHigh, hl) != CandleCompareResult.Lower)
        {
          result.Add(highs[i]);
        }
      }

      // Reverse back to chronological order
      result.Reverse();
      return result;
    }

    private List<Trendline> GetTrendLinesForTimeFrame(IEnumerable<Candle> candles, IList<int> highLows, TimeFrame tf)
    {
      var triplets = GetFirstOrderTrendlines(candles, highLows);
      var allOrders = FindAllOrdersTrendlinesApriori(triplets, out int maxK, parallelVerify: true);
      var maxorders = allOrders[maxK];
      var trendlines = new List<Trendline>(); 
      string tfStr = EnumMapper.GetTimeFrame(tf);
      foreach (var order in maxorders)
      {
        var startCandle = candles.ElementAt(order[0]);
        var endCandle = candles.ElementAt(order[^1]);
        var slopeRange = Utility.GetSlopeRange(startCandle, endCandle, order[0], order[^1], HighLowType.High);
      }
      return trendlines;
    }
    private List<int[]> GetFirstOrderTrendlines(IEnumerable<Candle> candles, IList<int> highs)
    {
      var triplets = new List<int[]>();
      int n = highs.Count;
      if (n < 2) return triplets; // Need at least two points to form a trendline

      for (int i = 0; i < n; i++)
      {
        for (int j = i + 1; j < n; j++)
        {
          SlopeRange slopeRange = Utility.GetSlopeRange(candles.ElementAt(highs[i]),
          candles.ElementAt(highs[j]), highs[i], highs[j], HighLowType.High);
          for (int k = j + 1; k < n; k++)
          {
            if (Utility.IsTrendLinePossible(candles.ElementAt(highs[i]),
             candles.ElementAt(highs[k]), highs[k] - highs[i], slopeRange, HighLowType.High))
            {
              triplets.Add(new int[] { highs[i], highs[j], highs[k] });
            }
          }
        }
      }

      return triplets;
    }
    // Given prevOrder (sorted int[] lists) and prevHash (keys), produce next-level sets (k = prevK+1)
    // parallelVerify: if true, candidate verification runs in parallel
    private List<int[]> GenerateNextOrderTrendlines(List<int[]> prevOrder, HashSet<string> prevHash, bool parallelVerify)
    {
        int prevK = prevOrder.Count > 0 ? prevOrder[0].Length : 0;
        if (prevK < 1) return new List<int[]>();
        int nextK = prevK + 1;
        var groups = new Dictionary<string, List<int[]>>();
        // Group by first (k-2) elements (for prevK==3, k-2 = 1)
        int prefixLen = prevK - 1; // k-2
        foreach (var s in prevOrder)
        {
            var prefix = string.Join('#', s.Take(prefixLen));
            if (!groups.TryGetValue(prefix, out var list)) { list = new List<int[]>(); groups[prefix] = list; }
            list.Add(s);
        }

        // Generate candidate keys (dedupe with HashSet)
        var candidateKeys = new HashSet<string>();
        var candidates = new List<int[]>();

        foreach (var kv in groups)
        {
            var list = kv.Value;
            int m = list.Count;
            // pairwise combine list items: they share first (k-2) elements by grouping
            for (int i = 0; i < m; i++)
            {
                for (int j = i + 1; j < m; j++)
                {
                    // union of two sorted arrays of length prevK, they differ only in last positions generally
                    // For safety, we'll create merged array and sort (cost is small compared to overall checks)
                    int[] merged = new int[nextK];
                    // copy elements from first
                    Array.Copy(list[i], 0, merged, 0, prevK);
                    // copy last element from second's tail (or just append then sort)
                    merged[prevK] = list[j][prevK - 1];
                    Array.Sort(merged);
                    string kkey = KeyOf(merged);
                    if (candidateKeys.Add(kkey))
                        candidates.Add(merged);
                }
            }
        }

        if (candidates.Count == 0) return new List<int[]>();

        var validated = new ConcurrentBag<int[]>();

        Action<int[]> verifyOne = candidate =>
        {
            // For candidate of length nextK, ensure every (nextK) subsets of length prevK exist in prevHash
            bool ok = true;
            // generate each subset by skipping one element
            for (int skip = 0; skip < nextK; skip++)
            {
                // build subset key quickly
                // avoid allocations by using a small array
                int idx = 0;
                var subset = new int[prevK];
                for (int t = 0; t < nextK; t++)
                {
                    if (t == skip) continue;
                    subset[idx++] = candidate[t];
                }
                string subkey = KeyOf(subset);
                if (!prevHash.Contains(subkey)) { ok = false; break; }
            }
            if (ok) validated.Add(candidate);
        };

        if (parallelVerify)
        {
            Parallel.ForEach(candidates, verifyOne);
        }
        else
        {
            foreach (var c in candidates) verifyOne(c);
        }

        return validated.ToList();
    }
    public Dictionary<int, List<int[]>> FindAllOrdersTrendlinesApriori(List<int[]> triplets, out int maxK, bool parallelVerify = true)
    {
        var levels = new Dictionary<int, List<int[]>>();
        maxK = 3;
        if (triplets == null || triplets.Count() == 0) return levels;

        levels[3] = triplets; 

        // build hash for prevOrder membership check
        var prevOrder = triplets;
        var prevHash = new HashSet<string>(prevOrder.Select(KeyOf));
        int k = 3;
        while (true)
        {
            var nextOrder = GenerateNextOrderTrendlines(prevOrder, prevHash, parallelVerify);
            if (nextOrder == null || nextOrder.Count == 0) break;
            k++;
            levels[k] = nextOrder;
            // prepare for next iteration
            prevOrder = nextOrder;
            prevHash = new HashSet<string>(prevOrder.Select(KeyOf));
        }

        return levels;
    }

    private static string KeyOf(int[] arr) => string.Join('#', arr);
    
  }
}