using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Shared.Constants;

namespace TradingApp.Shared.Services
{
  public static class Utility
  {
    private const double MAX_NUM = double.MaxValue;
    public static async Task<IEnumerable<Candle>> GetCandlesFromDB(int token, TimeFrame tf,
     IDailyTFRepository _dailyTF,
     IFifteenTFRepository _fifteenTF,
     DateTime from = default) {
      if (tf <= TimeFrame.FourHour)
        return await _fifteenTF.GetAllFifteenTFAsync(token, from);
      return await _dailyTF.GetAllDailyTFAsync(token, from);
    }
    public static List<Candle> Resample(TimeFrame timeFrame, IEnumerable<Candle> candles)
    {
      try
      {
        if (candles == null || !candles.Any())
          return new List<Candle>();

        var resampledCandles = new List<Candle>();

        // Direct copy for 15m or 1D
        if (timeFrame == TimeFrame.FifteenMinute || timeFrame == TimeFrame.Day)
        {
          foreach (var candle in candles)
          {
            resampledCandles.Add(new Candle(candle.token,
              candle.time, candle.open, candle.high, candle.low, candle.close, candle.volume));
          }
          return resampledCandles;
        }

        DateTime time = GetInitialTimeResample(candles.ElementAt(0).time, timeFrame);
        double low = MAX_NUM;
        double open = candles.ElementAt(0).open, high = 0, close = 0;
        int volume = 0;
        int token = candles.ElementAt(0).token;

        DateTime nextTime = GetNextTimeResample(time, timeFrame);

        foreach (var candle in candles)
        {
          if (candle.time >= time && candle.time < nextTime)
          {
            close = candle.close;
            low = Math.Min(low, candle.low);
            high = Math.Max(high, candle.high);
            volume += candle.volume;
          }
          else
          {
            if (open != 0)
            {
              resampledCandles.Add(new Candle(token, time, open, high, low, close, volume));
            }

            time = candle.time;
            nextTime = GetNextTimeResample(time, timeFrame);
            open = candle.open;
            high = candle.high;
            low = candle.low;
            close = candle.close;
            volume = 0;
          }
        }

        if (open != 0)
        {
          resampledCandles.Add(new Candle(token, time, open, high, low, close, volume));
        }

        return resampledCandles;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Exception in Resample: {ex.Message}");
        // Log the exception (you can use any logging framework)
        return new List<Candle>();
      }
    }
    public static DateTime GetNextTimeResample(DateTime currentTime, TimeFrame timeFrame)
    {
      switch (timeFrame)
      {
        case TimeFrame.FifteenMinute:
          return currentTime.AddMinutes(15);
        case TimeFrame.ThirtyMinute:
          return currentTime.AddMinutes(30);
        case TimeFrame.OneHour:
          return currentTime.AddHours(1);
        case TimeFrame.TwoHour:
          return currentTime.AddHours(2);
        case TimeFrame.FourHour:
          return currentTime.AddHours(4);
        case TimeFrame.Day:
          return currentTime.AddDays(1);
        case TimeFrame.Week:
          return currentTime.AddDays(7);
        case TimeFrame.Month:
          return currentTime.Month == 12
              ? new DateTime(currentTime.Year + 1, 1, 1)
              : new DateTime(currentTime.Year, currentTime.Month + 1, 1);
        default:
          return currentTime.AddDays(1);
      }
    }
    public static DateTime GetInitialTimeResample(DateTime currentTime, TimeFrame timeFrame)
    {
      // Assume trading day starts at 09:15 like in your Python version
      DateTime tradingStart = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 9, 15, 0);
      double minutesSinceStart = (currentTime - tradingStart).TotalMinutes;

      switch (timeFrame)
      {
        case TimeFrame.FifteenMinute:
          return currentTime;

        case TimeFrame.ThirtyMinute:
          return currentTime.AddMinutes(-(minutesSinceStart % 30));

        case TimeFrame.OneHour:
          return currentTime.AddMinutes(-(minutesSinceStart % 60));

        case TimeFrame.TwoHour:
          return currentTime.AddMinutes(-(minutesSinceStart % 120));

        case TimeFrame.FourHour:
          return currentTime.AddMinutes(-(minutesSinceStart % 240));

        case TimeFrame.Day:
          return currentTime.Date;

        case TimeFrame.Week:
          // Start of the week (Monday)
          return currentTime.AddDays(-((int)currentTime.DayOfWeek - (int)DayOfWeek.Monday));

        case TimeFrame.Month:
          return new DateTime(currentTime.Year, currentTime.Month, 1);

        default:
          return currentTime.Date;
      }
    }
    public static CandleCompareResult CompareCandles(Candle candle1, Candle candle2, HighLowType mode)
    {
      if (mode == HighLowType.High)
      {
        var c1HighVal = Math.Max(candle1.open, candle1.close);
        var c2HighVal = Math.Max(candle2.open, candle2.close);

        if (c1HighVal > candle2.high)
          return CandleCompareResult.Higher;
        else if (c2HighVal > candle1.high)
          return CandleCompareResult.Lower;
        else
          return CandleCompareResult.Equal;
      }
      if (mode == HighLowType.Low)
      {
        var c1LowVal = Math.Min(candle1.open, candle1.close);
        var c2LowVal = Math.Min(candle2.open, candle2.close);

        if (c1LowVal < candle2.low)
          return CandleCompareResult.Lower;
        else if (c2LowVal < candle1.low)
          return CandleCompareResult.Higher;
        else
          return CandleCompareResult.Equal;
      }
        return CandleCompareResult.Equal;
    }
    public static bool IsTrendLinePossible(Candle c1, Candle c3, int dx, SlopeRange slopeRange, HighLowType hl)
    {
      if (hl == HighLowType.High)
      {
        double c3HighVal = Math.Max(c3.open, c3.close);
        double c1HighVal = Math.Max(c1.open, c1.close);

        // Check upper bound
        if (c3HighVal > slopeRange.Max * dx + c1HighVal)
          return false;

        // Check lower bound
        if (c3HighVal < slopeRange.Min * dx + c1HighVal)
          return false;
      }
      else if (hl == HighLowType.Low)
      {
        double c3LowVal = Math.Min(c3.open, c3.close);
        double c1LowVal = Math.Min(c1.open, c1.close);

        // Check lower bound
        if (c3LowVal > slopeRange.Max * dx + c1LowVal)
          return false;

        // Check upper bound
        if (c3LowVal < slopeRange.Min * dx + c1LowVal)
          return false;
      }

      return true;
    }
    public static SlopeRange GetSlopeRange(Candle c1, Candle c2, int c1Index, int c2Index, HighLowType hl)
    {
      if (hl == HighLowType.High)
      {
        return new SlopeRange
        (
                (c2.high - Math.Max(c1.open, c1.close)) / (c2Index - c1Index),
                (c1.high - Math.Max(c2.open, c2.close)) / (c1Index - c2Index)
        );
      }
      else // 
      {
        return new SlopeRange
        (
                (c1.low - Math.Min(c2.open, c2.close)) / (c1Index - c2Index),
                (c2.low - Math.Min(c1.open, c1.close)) / (c2Index - c1Index)
        );
      }
    } 
    public static double GetIntercept(double slope, double y, int x)
    {
      return y - slope * x;
    }
    public static DateTime GetStartTimeOfAnalysis(TimeFrame timeFrame, DateTime? time = null)
    {
      DateTime t = time ?? DateTime.Now;

      switch (timeFrame)
      {
        case TimeFrame.FifteenMinute:
          return t.AddDays(-31);

        case TimeFrame.ThirtyMinute:
          return t.AddDays(-61);

        case TimeFrame.OneHour:
          return t.AddDays(-(14 * 7));   // 14 weeks

        case TimeFrame.TwoHour:
          return t.AddDays(-(28 * 7));   // 28 weeks

        case TimeFrame.FourHour:
          return new DateTime(t.Year, t.Month, t.Day).AddYears(-1);

        case TimeFrame.Day:
          return new DateTime(t.Year, t.Month, t.Day).AddYears(-2);

        case TimeFrame.Week:
        case TimeFrame.Month:
          return new DateTime(t.Year, t.Month, t.Day).AddYears(-20);

        default:
            return t;
      }
    }
    public static TimeFrame GetBaseTimeFrame(TimeFrame tf)
    {
      if (tf <= TimeFrame.FourHour)
        return TimeFrame.FifteenMinute;
      return TimeFrame.Day;
    }
    public static List<TimeFrame> GetAllTimeframes()
    {
      return new List<TimeFrame>
      {
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
    public static List<string> GetAllTimeframeStrings()
    {
      return new List<string>
      {
        "15m",
        "30m",
        "1h",
        "2h",
        "4h",
        "1D",
        "1W",
        "1M"
      };
    }
    public static int TimeFrameToMinutes(TimeFrame tf)
    {
      return tf switch
      {
        TimeFrame.FifteenMinute => 15,
        TimeFrame.ThirtyMinute => 30,
        TimeFrame.OneHour => 60,
        TimeFrame.TwoHour => 120,
        TimeFrame.FourHour => 240,
        TimeFrame.Day => 1440,
        TimeFrame.Week => 10080,
        TimeFrame.Month => 43200,
        _ => 1440,
      };
    }
    public static int TimeFrameCandlesPerDay(TimeFrame tf)
    {
      return tf switch
      {
        TimeFrame.FifteenMinute => 25,
        TimeFrame.ThirtyMinute => 13,
        TimeFrame.OneHour => 7,
        TimeFrame.TwoHour => 4,
        TimeFrame.FourHour => 2,
        TimeFrame.Day => 1,
        TimeFrame.Week => 0,
        TimeFrame.Month => 0,
        _ => 1,
      };
    }
    public static void GetToken(string symbol, out int token)
    {
      if (!AppConstants.StockLookUP.TryGetValue(symbol, out var tokenValue))
      {
        if (!int.TryParse(tokenValue, out token))
        {
          token = -1;
        }
      }
      else
      {
        token = -1;
      }
    }
  }
}