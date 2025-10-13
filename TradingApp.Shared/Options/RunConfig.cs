using System.Xml.Serialization;
using TradingApp.Shared.Constants;

namespace TradingApp.Shared.Options
{
  public class RunConfig
  {
    public string Mode { get; set; } = "Background";  
    public DebugOptions DebugOptions { get; set; } = new();
    public BackgroundOptions BackgroundOptions { get; set; } = new();
  }

  public class DebugOptions
  {
    public FetchServiceConfig FetchConfig{ get; set; } = new();
    public DataProcessingServiceConfig ProcessingConfig{ get; set; } = new();
    public AnalysisServiceConfig AnalysisConfig{ get; set; } = new();
    public DatabaseCleanUpServiceConfig CleanUpConfig{ get; set; } = new();
    public List<string> Symbols { get; set; } = new();
  }

  // Base class for all services
  public abstract class ServiceConfig
  {
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true; // If false, skip this service
  }

  // Example: Fetch Service
  public class FetchServiceConfig : ServiceConfig
  {
    [XmlIgnore]
    public FetchClient Client { get; set; } = FetchClient.Dhan;

    [XmlElement("client")]
    public string ClientString
    {
      get => EnumMapper.GetClient(Client);
      set => Client = EnumMapper.GetClient(value);
    }

    [XmlIgnore]
    public TimeFrame TimeFrame { get; set; } = TimeFrame.Day;

    [XmlElement("timeFrame")]
    public string TimeFrameString
    {
      get => EnumMapper.GetTimeFrame(TimeFrame);
      set => TimeFrame = EnumMapper.GetTimeFrame(value);
    }

    [XmlIgnore]
    public FetchType FetchType { get; set; } = FetchType.Latest;

    [XmlElement("fetchType")]
    public string FetchTypeString
    {
      get => EnumMapper.GetFetchType(FetchType);
      set => FetchType = EnumMapper.GetFetchType(value);
    }
  }

  // Example: Data Processing Service
  public class DataProcessingServiceConfig : ServiceConfig
  {
  }

  // Example: Analysis Service
  public class AnalysisServiceConfig : ServiceConfig
  {
    public string StrategyName { get; set; } = "DefaultStrategy";
    public double RiskThreshold { get; set; } = 0.05;
  }

  public class DatabaseCleanUpServiceConfig : ServiceConfig
  {
    [XmlIgnore]
    public List<Table> TablesToClean { get; set; } = new();

    [XmlArray("Tables")]
    [XmlArrayItem("Table")]
    public string[] TableStrings
    {
      get => TablesToClean.Select(EnumMapper.GetTable).ToArray();
      set
      {
        Console.WriteLine($"TableStrings.setter called; value is {(value == null ? "null" : $"{value.Length} items")}");
        if (value != null)
        {
            for (int i = 0; i < value.Length; i++)
                Console.WriteLine($"  [{i}]='{value[i]}'");
        }

        if (value == null)
        {
            TablesToClean = new List<Table>();
            return;
        }

        var list = new List<Table>();
        foreach (var s in value)
        {
            if (string.IsNullOrWhiteSpace(s)) continue;
            list.Add(EnumMapper.GetTable(s)); // assume GetTable(string) now strips _data
        }

        TablesToClean = list;
      }
    }
  }

  public class BackgroundOptions
  {
    public TimeSpan StartTime { get; set; } = new(16, 30, 0);
    public List<DayOfWeek> Days { get; set; } = new()
    {
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday
    };
  }
}
