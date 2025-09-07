using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradingApp.Shared.Options;
using TradingApp.Shared.Services;

namespace TradingApp.Processor.Workers
{
  public class DebugWorker : BackgroundService
  {
    private readonly ILogger<DebugWorker> _logger;
    private readonly DebugOptions _config;
    private readonly DataFetchService _fetchService;
    private readonly DataProcessingService _processService;
    private readonly AnalysisService _analysisService;

    public DebugWorker(
        ILogger<DebugWorker> logger,
        RunConfig config,
        DataFetchService fetchService,
        DataProcessingService processService,
        AnalysisService analysisService)
    {
      _logger = logger;
      _config = config.DebugOptions;
      _fetchService = fetchService;
      _processService = processService;
      _analysisService = analysisService;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {

      if (_config.FetchConfig.Enabled)
          await _fetchService.ExecuteAsync(ct);

      if (_config.FetchConfig.Enabled)
          await _processService.ExecuteAsync(ct);

      if (_config.FetchConfig.Enabled)
          await _processService.ExecuteAsync(ct);      
    }
  }
}
