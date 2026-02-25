using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure.Data;
using TradingApp.Shared.Constants;
using TradingApp.Shared.ExternalApis;
using TradingApp.Shared.Options;

namespace TradingApp.Shared.Services
{
  public class StockDetailsFetcher : IService
  {
    private readonly ILogger<TableInitializationService> _logger;
    private readonly TableInitializationServiceConfig _config;
    private readonly TradingDbContext _dbContext;
    private readonly IMarketApiClient<DailyTF> _client;
    private readonly DhanScanSettings scanSettings;

    public StockDetailsFetcher(TradingDbContext dbContext, RunConfig runConfig,
      IMarketApiFactory<DailyTF> factory,
      IOptions<DhanScanSettings> scanOptions,
      ILogger<TableInitializationService> logger)
    {
      _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
      _config = runConfig?.DebugOptions?.InitConfig ?? new TableInitializationServiceConfig();
      _logger = logger;
      _client = factory.GetClient(EnumMapper.GetClient(FetchClient.Dhan));      
      scanSettings = scanOptions.Value;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
      _logger.LogInformation("StockDetailsFetcher started...");

      // Ensure database and all mapped tables are created based on the current EF Core model.
      try
      {
        // Prefer migrations in production; EnsureCreated is simple and will create whatever the model defines.
        var data = await _client.FetchScanDataAsync(scanSettings, ct);
        await _dbContext.Database.EnsureCreatedAsync(ct);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error while Stock details fetcher");
        // continue so we can attempt per-table checks
      }
      _logger.LogInformation("StockDetailsFetcher completed.");
    }
  }
}
