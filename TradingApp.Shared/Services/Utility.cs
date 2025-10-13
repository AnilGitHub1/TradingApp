using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Shared.Constants;

namespace TradingApp.Shared.Services
{
  public static class Utility
  {
    private const double MAX_NUM = double.MaxValue;
    public static async Task<IEnumerable<Candle>> GetCandlesFromDB(string symbol, TimeFrame tf,
     IDailyTFRepository _dailyTF,
     IFifteenTFRepository _fifteenTF) {
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

    public static TimeFrame GetBaseTimeFrame(TimeFrame tf)
    {
      if (tf <= TimeFrame.FourHour)
        return TimeFrame.FifteenMinute;
      return TimeFrame.Day;      
    }
    public static List<TimeFrame> GetAllTimeframes()
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