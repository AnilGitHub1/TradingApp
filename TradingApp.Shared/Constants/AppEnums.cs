namespace TradingApp.Shared.Constants
{
  public enum FetchClient
  {
    Dhan,
  }

  public enum CandleType
  {
    High,
    Low
  }

  public enum TimeFrame
  {
    FifteenMinute,
    ThirtyMinute,
    OneHour,
    TwoHour,
    FourHour,
    Day,
    Week,
    Month,
  }

  public static class EnumMapper
  {
    public static string GetTimeFrame(TimeFrame tf)
    {
      switch (tf)
      {
        case TimeFrame.FifteenMinute:
          return "15m";
        case TimeFrame.ThirtyMinute:
          return "30m";
        case TimeFrame.OneHour:
          return "1H";
        case TimeFrame.TwoHour:
          return "2H";
        case TimeFrame.FourHour:
          return "4H";
        case TimeFrame.Day:
          return "1D";
        case TimeFrame.Week:
          return "1W";
        case TimeFrame.Month:
          return "1M";
        default:
          return "1D";
      }
    }
    public static string GetClient(FetchClient client)
    {
      switch (client)
      {
        case FetchClient.Dhan:
          return "Dhan";
        default:
          return "";
      }
    }
    public static TimeFrame GetTimeFrame(string tf)
    {
      tf = tf.ToLower();
      tf = tf.Replace(" ","");
      switch (tf)
      {
        case "15m":
        case "15min":
        case "fifteenminute":
          return TimeFrame.FifteenMinute;
        case "30m":
        case "30min":
        case "thirtyminute":
          return TimeFrame.ThirtyMinute;
        case "1h":
        case "h":
        case "hour":
        case "onehour":
          return TimeFrame.OneHour;
        case "2h":
        case "twohour":
          return TimeFrame.TwoHour;
        case "4h":
        case "fourhour":
          return TimeFrame.FourHour;
        case "d":
        case "1d":
        case "day":
        case "oneday":
          return TimeFrame.Day;
        case "w":
        case "1w":
        case "week":
        case "oneweek":
          return TimeFrame.Week;
        case "m":
        case "1m":
        case "month":
        case "onemonth":
          return TimeFrame.Month;
        default:
          return TimeFrame.Day;
      }
    }
    public static FetchClient GetClient(string client)
    {
      client = client.ToLower();
      client = client.Replace(" ","");
      switch (client)
      {
        case "dhan":
          return FetchClient.Dhan;
        default:
          return FetchClient.Dhan;
      }
    }
  }  
}