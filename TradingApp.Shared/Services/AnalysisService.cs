using Microsoft.Extensions.Logging;
using TradingApp.Core.Contracts;
using TradingApp.Core.Interfaces;
using TradingApp.Shared.Options;
using TradingApp.Shared.Constants;
using TradingApp.Core.Entities;
using System.Collections.Concurrent;
using OR;
using TradingApp.Shared.Helpers;

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
    private readonly HyperCliqueFinder _hyperCliqueFinder;
    private readonly Apriori _apriori;
    private readonly ILogger<AnalysisService> _logger;
    private int CurrentToken;

    public AnalysisService(IDailyTFRepository dailyTF, IFifteenTFRepository fifteenTF,
    RunConfig cfg, ITrendlineRepository trendlineRepo, IHighLowRepository highLowRepo, ILogger<AnalysisService> logger)
    {
      _dailyTF = dailyTF;
      _fifteenTF = fifteenTF;
      _cfg = cfg.DebugOptions.AnalysisConfig;
      _symbols = cfg.DebugOptions.Symbols;
      _highLowRepo = highLowRepo;
      _trendlineRepo = trendlineRepo;
      _hyperCliqueFinder = new HyperCliqueFinder();
      _apriori = new Apriori();
      _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
      _logger.LogInformation("AnalysisService started with config: {@Config}", _cfg.ToString());
      List<TimeFrame> timeFrames = Utility.GetAllTimeframes();
      var results = new AnalysisResult(new List<Trendline>());
      foreach (var symbol in _symbols)
      {
        Utility.GetToken(symbol, out int token);
        if(token == -1)
        {
          _logger.LogWarning("Token not found for symbol {Symbol}. Skipping.", symbol);
          continue;
        }
        CurrentToken = token;
        if (ct.IsCancellationRequested) break;
        _logger.LogInformation("Analysing stock: {Symbol}", symbol);
        IEnumerable<Candle> candles = new List<Candle>();            
        IList<HighLow> highLows = new List<HighLow>();

        foreach (var tf in timeFrames)
        {
          _logger.LogInformation("Analysing symbol: {Symbol} for timeframe: {TimeFrame}", symbol, tf);
          DateTime from = Utility.GetStartTimeOfCandlesForAnalysis(tf);
          if(tf == TimeFrame.FifteenMinute || tf == TimeFrame.Day)
          { 
            highLows = await Utility.GetHighLows(token, tf, _highLowRepo, from);
          }
          candles = await Utility.GetCandles(token, tf, candles, _dailyTF, _fifteenTF, from);
          var highLowCandles = GetHighLowCandles(candles, highLows, tf, HighLowType.High);
          var highsForDownTrendLines = GetHighsForDownTrendLines(highLowCandles, candles, HighLowType.High);
          GetTrendLinesForTimeFrame(candles, highsForDownTrendLines, tf, results);
        }
        _logger.LogInformation("Completed processing for symbol: {Symbol}", symbol);
      }
      if (results.Trendlines.Count > 0)
        await _trendlineRepo.UpdateTrendlinesAsync(results.Trendlines.ToList());
    }   
    public IList<int> GetHighLowCandles(IEnumerable<Candle> candles, IList<HighLow> highLows, TimeFrame tf, HighLowType hl)
    {
      if (candles == null || !candles.Any() || highLows == null || !highLows.Any())
        return new List<int>();

      var highLowCandles = new List<int>();
      string hlStr = hl == HighLowType.High ? "h" : "l";
      string tfStr = EnumMapper.GetTimeFrame(tf);
      var highLowSet = new HashSet<DateTime>();
      for(int i = 0; i< highLows.Count; i++)
      {
        if(highLows[i].tf.Contains(tfStr) && highLows[i].hl.Contains(hlStr))
        {
          highLowSet.Add(highLows[i].time);
        }
      }
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
    private void GetTrendLinesForTimeFrame(IEnumerable<Candle> candles, IList<int> highLows, TimeFrame tf, AnalysisResult results)
    {
      var triplets = GetFirstOrderTrendlines(candles, highLows);
      if(triplets.Count() == 0) {
        _logger.LogInformation("no Triplets...");
        return;
      }
      if(_cfg.PrintTriplets) PrintAllTriplets(triplets);
      var allOrders = FindAllOrders(triplets, out var maxK);
      var maxorders = allOrders[maxK];
      if(_cfg.PrintResults) PrintResults(maxorders);
      string tfStr = EnumMapper.GetTimeFrame(tf);
      foreach (var order in maxorders)
      {

        Trendline trendline = CreateModelAndSolve(candles, order);
        if (trendline.slope == 0.0 && trendline.intercept == 0.0)
          continue;
        else
        {
          trendline.token = CurrentToken;
          trendline.tf = tfStr;
          results.Trendlines.Add(trendline);
          break;
        }
      }
    }
    private Dictionary<int,List<int[]>> FindAllOrders(List<int[]> triplets, out int maxK)
    {
      var allOrders = new Dictionary<int, List<int[]>>();
      maxK = 3;
      switch (_cfg.AlgoType)
      {
        case AnalysisAlgoType.Apriori:
          return _apriori.FindAllOrdersTrendlinesApriori(triplets, out maxK, parallelVerify: true);
        case AnalysisAlgoType.HyperClique:
          return _hyperCliqueFinder.FindAllLevelsFromTriplets(triplets, out  maxK);
        case AnalysisAlgoType.HyperCliqueMaxOnly:
          return _hyperCliqueFinder.FindAllLevelsFromTriplets(triplets, out  maxK, true);
        default:
          allOrders.Add(3, new List<int[]>());
          return allOrders;
      }
    }
    private Trendline CreateModelAndSolve(IEnumerable<Candle> candles, IList<int> highs)
    {
      if (candles.Count() == 0 || highs.Count == 0)
        return Trendline.EmptyTrendline();
      var model = new MipModel();
      var startCandle = candles.ElementAt(highs[0]);
      var endCandle = candles.ElementAt(highs[^1]);    
      var slopeRange = Utility.GetSlopeRange(startCandle, endCandle, highs[0], highs[^1], HighLowType.High);
      var slopeVar = model.AddVariable("slope", slopeRange.Min, slopeRange.Max, MipVarType.Continuous);
      var interceptVar = model.AddVariable("intercept", Utility.GetIntercept(slopeRange.Max, endCandle.high, highs[^1]),
      Utility.GetIntercept(slopeRange.Min, startCandle.high, highs[0]), MipVarType.Continuous);
      var highVars = new Dictionary<int, MipVariable>();
      foreach (var hi in highs)
      {
        // Create variables 
        var highVar = model.AddVariable("high_" + hi, 0, 1, MipVarType.Binary, 0.0);
        highVars[hi] = highVar;
        var highCandle = candles.ElementAt(hi);
        // Constraint:  slope * hi + intercept <= highCandle.high
        var lessThanConstraint = model.AddConstraint($"insideWick_{hi}", ConstraintSense.LessOrEqual, highCandle.high);
        lessThanConstraint.AddTerm(slopeVar.Id, hi);
        lessThanConstraint.AddTerm(interceptVar.Id, 1.0);
        // Constraint: slope * hi + intercept >= max(open, close)
        var higherThanConstraint = model.AddConstraint($"aboveBody_{hi}", ConstraintSense.GreaterOrEqual,
         Math.Max(highCandle.open, highCandle.close));
        higherThanConstraint.AddTerm(slopeVar.Id, hi);
        higherThanConstraint.AddTerm(interceptVar.Id, 1.0);
      }
      model.SetObjective(ObjectiveSense.Minimize,
      new Dictionary<int, double>()
      {
        { slopeVar.Id, highs.Sum(x=>x)},
        { interceptVar.Id, highs.Count}
      });
      var sol = MipSolver.SolveWithOrTools(model);
      if (!sol.IsOptimal)
        return Trendline.EmptyTrendline();
      double m = sol.VariableValues[slopeVar.Id];
      double c = sol.VariableValues[interceptVar.Id];

      // Validate 
      foreach (var hi in highs)
      {
        var ch = candles.ElementAt(hi);
        double y = m * hi + c;
        if (y > ch.high + 1e-9)
        {
          _logger.LogWarning($"Trendline validation failed at high {ch.ToString()}: calculated y {y} exceeds candle high {ch.high}");
          return Trendline.EmptyTrendline();
        }
        if (y < Math.Max(ch.open, ch.close) - 1e-9)
        {
          _logger.LogWarning($"Trendline validation failed at high {ch.ToString()}: calculated y {y} below candle body max {Math.Max(ch.open, ch.close)}");
          return Trendline.EmptyTrendline();
        }
      }
      return new Trendline(
        token: -1,
        starttime: startCandle.time,
        endtime: endCandle.time,
        slope: m,
        intercept: c,
        hl: "h",
        tf: "",
        index: -1,
        index1: highs[0],
        index2: highs[^1],
        connects: highs.Count,
        totalconnects: highs.Count
      );
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
              triplets.Add([highs[i], highs[j], highs[k]]);
            }
          }
        }
      }

      return triplets;
    }
    private void PrintAllTriplets(List<int[]> triplets)
    {
      foreach(var triplet in triplets)
      {
        _logger.LogInformation("("+triplet[0]+","+triplet[1]+","+triplet[2]+")");
      }
    }
    private void PrintResults(List<int[]> maxOrders)
    {
      _logger.LogInformation("results " + EnumMapper.GetAnalysisAlgoType(_cfg.AlgoType));
      foreach(var order in maxOrders)
      {
        string msg = "(";
        foreach(var i in order) msg += ","+i;
        msg += ")";
        _logger.LogInformation(msg);
      }
    }
  }
}