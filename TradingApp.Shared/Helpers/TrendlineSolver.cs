using OR;
using TradingApp.Core.Entities;
using TradingApp.Shared.Constants;
using TradingApp.Shared.Services;

namespace TradingApp.Shared.Helpers
{
  public static class TrendlineSolver
  {
    public static Trendline CreateModelAndSolve(IEnumerable<Candle> candles, IList<int> highs)
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
          // _logger.LogWarning($"Trendline validation failed at high {ch.ToString()}: calculated y {y} exceeds candle high {ch.high}");
          return Trendline.EmptyTrendline();
        }
        if (y < Math.Max(ch.open, ch.close) - 1e-9)
        {
          // _logger.LogWarning($"Trendline validation failed at high {ch.ToString()}: calculated y {y} below candle body max {Math.Max(ch.open, ch.close)}");
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

  }
}