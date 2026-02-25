using TradingApp.Shared.Services;

namespace TradingApp.Shared.Constants
{
  public enum AnalysisAlgoType
  {
    Apriori,
    HyperClique,
    HyperCliqueMaxOnly
  }
  public enum FetchClient
  {
    Dhan,
  }

  public enum HighLowType
  {
    High,
    Low,
    HighLow
  }

  public enum TimeFrame
  {
    FifteenMinute = 0,
    ThirtyMinute = 1,
    OneHour = 2,
    TwoHour = 3,
    FourHour = 4,
    Day = 5,
    Week = 6,
    Month = 7,
  }

  public enum Table
  {
    DailyTF,
    FifteenTF,
    TrendLine,
    HighLow,
    Trade,
    Simulation
  }

  public enum FetchType
  {
    All,
    Latest
  }

  public enum CandleCompareResult { Lower, Equal, Higher }
  
  public enum StockCategory
  {
    ALL,
    NIFTY_50,
    NIFTY_100,
    NIFTY_200,
    NIFTY_500,
  }

  public enum StockSort
  {
    Alphabetic,
    Mcap,
    Trendline
  }
  public static class EnumMapper
  {
    public static string GetAnalysisAlgoType(AnalysisAlgoType type)
    {
      switch (type)
      {
        case AnalysisAlgoType.Apriori:
          return "Apriori";
        case AnalysisAlgoType.HyperClique:
          return "HyperClique";
        case AnalysisAlgoType.HyperCliqueMaxOnly:
          return "HyperCliqueMaxOnly";
        default:
          return "HyperClique";
      }
    }
    public static AnalysisAlgoType GetAnalysisAlgoType(string type)
    {
      type = type.ToLower();
      type = type.Replace(" ","");
      switch (type)
      {
        case "apriori":
          return AnalysisAlgoType.Apriori;
        case "hypercliquemaxonly":
          return AnalysisAlgoType.HyperCliqueMaxOnly;
        case "hyperclique":
          return AnalysisAlgoType.HyperClique;
        default:
          return AnalysisAlgoType.HyperCliqueMaxOnly;
      }
    }
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
      tf = Utility.TrimStartEndToLowerRemoveSpaces(tf);
      tf = tf.Replace("_","");
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
      client = client.Replace(" ", "");
      switch (client)
      {
        case "dhan":
          return FetchClient.Dhan;
        default:
          return FetchClient.Dhan;
      }
    }
    public static string GetTable(Table table)
    {
      switch (table)
      {
        case Table.DailyTF:
          return "dailytf_data";
        case Table.FifteenTF:
          return "fifteentf_data";
        case Table.TrendLine:
          return "trendline_data";
        case Table.HighLow:
          return "highlow_data";
        case Table.Trade:
          return "trade_data";
        case Table.Simulation:
          return "simulation_data";
        default:
          return "dailytf_data";
      }
    }
    public static string GetTable(TimeFrame tf)
    {
      if (tf <= TimeFrame.FourHour) return GetTable(Table.FifteenTF);
      return GetTable(Table.DailyTF);
    }
    public static Table GetTable(string table)
    {
      table = table.ToLower();
      table = table.Replace(" ", "");
      switch (table)
      {
        case "dailytf_data":
        case "dailytf":
        case "dailytfdata":
        case "daily_tf":
        case "dailydata":
        case "daily":
          return Table.DailyTF;
        case "fifteentf_data":
        case "fifteentf":
        case "fifteentfdata":
        case "fifteen_tf":
        case "fifteendata":
        case "fifteen":
          return Table.FifteenTF;
        case "trendline_data":
        case "trendline":
        case "trendlinedata":
          return Table.TrendLine;
        case "highlow_data":
        case "highlow":
        case "highlowdata":
          return Table.HighLow;
        case "trade_data":
        case "trade":
        case "tradedata":
          return Table.Trade;
        case "simulation_data":
        case "simulation":
        case "simulationdata":
          return Table.Simulation;
        default:
          return Table.DailyTF;
      }
    }
    public static FetchType GetFetchType(string fetchType)
    {
      fetchType = fetchType.ToLower();
      fetchType = fetchType.Replace(" ", "");
      switch (fetchType)
      {
        case "all":
          return FetchType.All;
        case "latest":
          return FetchType.Latest;
        default:
          return FetchType.Latest;
      }
    }
    public static string GetFetchType(FetchType fetchType)
    {
      switch (fetchType)
      {
        case FetchType.All:
          return "A";
        case FetchType.Latest:
          return "Latest";
        default:
          return "Latest";
      }
    }
    public static string GetStockCategory(StockCategory category)
    {
      switch (category)
      {
        case StockCategory.ALL:
        return "all";
        case StockCategory.NIFTY_50:
        return "nifty_50";
        case StockCategory.NIFTY_100:
        return "nifty_100";
        case StockCategory.NIFTY_200:
        return "nifty_200";
        case StockCategory.NIFTY_500:
        return "nifty_500";
        default:
        return "nifty_50";

      }
    }
    public static StockCategory GetStockCategory(string category)
    {
      category = Utility.TrimStartEndToLowerRemoveSpaces(category);
      switch (category)
      {
        case "all":
          return StockCategory.ALL;
        case "nifty":
        case "nifty_50":
        case "50":
        case "n50":
          return StockCategory.NIFTY_50;
        case "nifty_100":
        case "100":
        case "n100":
          return StockCategory.NIFTY_100;
        case "nifty_200":
        case "200":
        case "n200":
          return StockCategory.NIFTY_200;
        case "nifty_500":
        case "500":
        case "n500":
          return StockCategory.NIFTY_500;
        default:
          return StockCategory.NIFTY_50;
        
      }
    }
    public static string GetStockSortOrder(StockSort sort)
    {
      switch (sort)
      {
        case StockSort.Alphabetic:
        return "alpha";
        case StockSort.Mcap:
        return "mcap";
        case StockSort.Trendline:
        return "trendline";
        default:
        return "trendline";

      }
    }
    public static StockSort GetStockSortOrder(string sort)
    {
      sort = Utility.TrimStartEndToLowerRemoveSpaces(sort);
      switch (sort)
      {
        case "alpha":
        case "alphabetic":
        case "a":
          return StockSort.Alphabetic;
        case "1":
        case "mcap":
        case "cap":
        case "marketcap":
          return StockSort.Mcap;
        case "trend":
        case "trendline":
        case "touches":
        case "connects":
          return StockSort.Trendline;
        default:
          return StockSort.Trendline;        
      }
    }
  
  }  
}