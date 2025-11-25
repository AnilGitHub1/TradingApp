using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Shared.Constants;
using TradingApp.Shared.Options;
using TradingApp.Shared.Services;

namespace TradingApp.Processor.Workers
{
  public class DebugWorker : BackgroundService
  {
    private readonly ILogger<DebugWorker> _logger;
    private readonly DebugOptions _config;
    private readonly List<IService> _services = [];

    public DebugWorker(
        ILogger<DebugWorker> logger,
        RunConfig config,
        DataFetchService<DailyTF> fetchServiceDaily,
        DataFetchService<FifteenTF> fetchServiceFifteen,
        DataProcessingService processService,
        AnalysisService analysisService,
        DatabaseCleanUpService cleanUpService,
        TableInitializationService tableInitService
        )
    {
      _logger = logger;
      _config = config.DebugOptions;

      if (_config.FetchConfig.Enabled)
      {
        if (fetchServiceDaily != null && _config.FetchConfig.TimeFrame == TimeFrame.Day)
          _services.Add(fetchServiceDaily);
        else if (fetchServiceFifteen != null && _config.FetchConfig.TimeFrame == TimeFrame.FifteenMinute)
          _services.Add(fetchServiceFifteen);
      }
      if (processService != null && _config.ProcessingConfig.Enabled) _services.Add(processService);
      if (analysisService != null && _config.AnalysisConfig.Enabled) _services.Add(analysisService);
      if (cleanUpService != null && _config.CleanUpConfig.Enabled) _services.Add(cleanUpService);
      if (tableInitService != null && _config.InitConfig.Enabled) _services.Add(tableInitService);
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
      _logger.LogInformation("DebugWorker running at: {time}", DateTimeOffset.Now);
      foreach (var service in _services)
      {
        await service.ExecuteAsync(ct);
      }
      _logger.LogInformation("DebugWorker finished at: {time}", DateTimeOffset.Now);
    }
  }
}
