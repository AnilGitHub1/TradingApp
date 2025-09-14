using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradingApp.Core.Entities;
using TradingApp.Shared.Constants;
using TradingApp.Shared.Options;
using TradingApp.Shared.Services;

namespace TradingApp.Processor.Workers
{
  public class DebugWorker : BackgroundService
  {
    private readonly ILogger<DebugWorker> _logger;
    private readonly DebugOptions _config;
    private readonly DataFetchService<DailyTF> _fetchServiceDaily;
    private readonly DataFetchService<FifteenTF> _fetchServiceFifteen;
    private readonly DataProcessingService _processService;
    private readonly AnalysisService _analysisService;

    public DebugWorker(
        ILogger<DebugWorker> logger,
        RunConfig config,
        DataFetchService<DailyTF> fetchServiceDaily,
        DataFetchService<FifteenTF> fetchServiceFifteen,
        DataProcessingService processService,
        AnalysisService analysisService)
    {
      _logger = logger;
      _config = config.DebugOptions;
      _fetchServiceDaily = fetchServiceDaily;
      _fetchServiceFifteen = fetchServiceFifteen;
      _processService = processService;
      _analysisService = analysisService;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {

      if (_config.FetchConfig.Enabled && _config.FetchConfig.timeFrame == TimeFrame.Day)
          await _fetchServiceDaily.ExecuteAsync(ct);

      if (_config.FetchConfig.Enabled && _config.FetchConfig.timeFrame == TimeFrame.FifteenMinute)
          await _fetchServiceFifteen.ExecuteAsync(ct);

      if (_config.FetchConfig.Enabled)
          await _processService.ExecuteAsync(ct);

      if (_config.FetchConfig.Enabled)
          await _processService.ExecuteAsync(ct);      
    }
  }
}
