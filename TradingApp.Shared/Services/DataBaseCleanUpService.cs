using Microsoft.Extensions.Logging;
using TradingApp.Infrastructure.Data;
using TradingApp.Core.Contracts;
using TradingApp.Core.Interfaces;
using TradingApp.Shared.Options;
using Microsoft.EntityFrameworkCore;
using TradingApp.Shared.Constants;

namespace TradingApp.Shared.Services
{
  public class DatabaseCleanUpService : IService
  {
    private readonly TradingDbContext _db;
    private readonly DatabaseCleanUpServiceConfig _config;
    private readonly ILogger<DatabaseCleanUpService> _logger;

    public DatabaseCleanUpService(TradingDbContext db, RunConfig cfg, ILogger<DatabaseCleanUpService> logger)
    {
        _db = db;
        _config = cfg.DebugOptions.CleanUpConfig;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
      if (_config.Enabled)
      {
        try
        {
          if (_config.TablesToClean.Count == 0)
          {
            _logger.LogWarning("No tables specified for cleanup.");
            return;
          }
          _logger.LogInformation("Starting database cleanup...");
          // Collect all table names from config
          var tableNames = _config.TablesToClean
              .Select(EnumMapper.GetTable)
              .Select(name => $"{name}"); // quote table names for safety
          System.Console.WriteLine(tableNames);
          // Join them into one TRUNCATE command
          var sql = $"TRUNCATE TABLE {string.Join(", ", tableNames)} RESTART IDENTITY CASCADE;";
          sql = @"
                DO $$
                DECLARE
                    part text;
                BEGIN
                    FOR part IN
                        SELECT inhrelid::regclass::text
                        FROM pg_inherits
                        WHERE inhparent::regclass::text IN ('dailytf_data','fifteentf_data','highlow_data')
                    LOOP
                        EXECUTE format('TRUNCATE TABLE %I RESTART IDENTITY CASCADE;', part);
                    END LOOP;
                END$$;
                ";
          System.Console.WriteLine(sql);
          _logger.LogInformation($"Clearing tables: {string.Join(", ", tableNames)}");
          await _db.Database.ExecuteSqlRawAsync(sql, ct);
          _logger.LogInformation("All tables cleared.");
          _logger.LogInformation("Database cleanup completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Truncate failed ");
            throw;
        }        
      }
      else
      {
        _logger.LogInformation("Database cleanup service is disabled.");
        return;
      }
        
    }
  }
}