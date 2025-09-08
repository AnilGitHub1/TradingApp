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
    public FetchMode fetchMode = FetchMode.Latest;
    public TimeFrame timeFrame = TimeFrame.Day;
  }

  // Example: Data Processing Service
  public class DataProcessingServiceConfig : ServiceConfig
  {
    public bool UseParallelProcessing { get; set; } = false;
    public string OutputDirectory { get; set; } = "output";
  }

  // Example: Analysis Service
  public class AnalysisServiceConfig : ServiceConfig
  {
    public string StrategyName { get; set; } = "DefaultStrategy";
    public double RiskThreshold { get; set; } = 0.05;
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
