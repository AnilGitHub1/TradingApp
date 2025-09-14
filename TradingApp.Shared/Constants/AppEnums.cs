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
      switch (tf)
      {
        case "15m":
        case "15M":
          return TimeFrame.FifteenMinute;
        case "30m":
        case "30M":
          return TimeFrame.ThirtyMinute;
        case "1h":
        case "1H":
        case "h":
        case "H":
          return TimeFrame.OneHour;
        case "2h":
        case "2H":
          return TimeFrame.TwoHour;
        case "4h":
        case "4H":
          return TimeFrame.FourHour;
        case "D":
        case "1D":
        case "d":
        case "1d":
          return TimeFrame.Day;
        case "W":
        case "1W":
        case "w":
        case "1w":
          return TimeFrame.Week;
        case "M":
        case "1M":
        case "m":
        case "1m":
          return TimeFrame.Month;
        default:
          return TimeFrame.Day;
      }
    }
    public static FetchClient GetClient(string client)
    {
      switch (client)
      {
        case "dhan":
        case "Dhan":
          return FetchClient.Dhan;
        default:
          return FetchClient.Dhan;
      }
    }
  }
  
}