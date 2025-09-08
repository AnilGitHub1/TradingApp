using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using TradingApp.Core.Interfaces;
using TradingApp.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using TradingApp.Shared.ExternalApis;
using TradingApp.Shared.Options;
using TradingApp.Shared.Constants;

namespace TradingApp.Processor.Workers
{
    public class MarketCloseWorker : BackgroundService
    {
        private readonly ILogger<MarketCloseWorker> _logger;
        private readonly IServiceProvider _sp;
        private readonly IAppLogger<MarketCloseWorker> _alogger;
    private readonly BackgroundOptions _cfg;

        public MarketCloseWorker(ILogger<MarketCloseWorker> logger,
                             IServiceProvider sp,
                             IAppLogger<MarketCloseWorker> alogger,
                             RunConfig config)
    {
      _cfg = config.BackgroundOptions;
      _logger = logger;
      _sp = sp;
      _alogger = alogger;
    }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MarketCloseWorker running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = new DateTime(now.Year, now.Month, now.Day, _cfg.StartTime.Hours, _cfg.StartTime.Minutes, 0);
                if (nextRun <= now) nextRun = nextRun.AddDays(1);

                var delay = nextRun - now;
                _logger.LogInformation("Next run scheduled at {time} (in {delay}).", nextRun, delay);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                    await RunJobOnce(stoppingToken);
                }
                catch (TaskCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in worker loop.");
                }
            }

            _logger.LogInformation("MarketCloseWorker stopping.");
        }

        private async Task RunJobOnce(CancellationToken ct)
        {
            using var scope = _sp.CreateScope();
            var fetch = scope.ServiceProvider.GetRequiredService<DataFetchService>();
            var proc = scope.ServiceProvider.GetRequiredService<DataProcessingService>();
            var analysis = scope.ServiceProvider.GetRequiredService<AnalysisService>();

            List<string> symbols;
            string timeframe;
            symbols = AppConstants.AllTokens.Keys.ToList();
            timeframe = "1D"; 

            var providerName = (scope.ServiceProvider.GetRequiredService<IConfiguration>()["ExternalApi:DefaultProvider"] ?? "Alpha");

            var allFetched = new List<TradingApp.Core.Contracts.FetchResult>();
            foreach (var s in symbols)
            {
                ct.ThrowIfCancellationRequested();

                // Choose provider per config or per-symbol if you want
                var factory = scope.ServiceProvider.GetRequiredService<IMarketApiFactory>();
                var client = factory.GetClient(providerName);

                var fetched = await client.FetchAsync(s, timeframe, ct);
                if (fetched != null) allFetched.Add(fetched);
            }

            await proc.ExecuteAsync(ct);

            // Analyze with limited parallelism
            var degree = Math.Max(1, 4);
            var throttler = new SemaphoreSlim(degree);
            var tasks = new List<Task>();
            foreach (var fetched in allFetched)
            {
                await throttler.WaitAsync(ct);
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using var innerScope = _sp.CreateScope();
                        var innerAnalysis = innerScope.ServiceProvider.GetRequiredService<AnalysisService>();
                        await innerAnalysis.ExecuteAsync(ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error analyzing symbol {symbol}","");
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }, ct));
            }

            await Task.WhenAll(tasks);
            _logger.LogInformation("Run completed.");
        }
    }
}
