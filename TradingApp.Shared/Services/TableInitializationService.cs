using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure.Data;
using TradingApp.Shared.Constants;
using TradingApp.Shared.Options;

namespace TradingApp.Shared.Services
{
  public class TableInitializationService : IService
  {
    private readonly ILogger<TableInitializationService> _logger;
    private readonly TableInitializationServiceConfig _config;
    private readonly TradingDbContext _dbContext;

    public TableInitializationService(TradingDbContext dbContext, RunConfig runConfig, ILogger<TableInitializationService> logger)
    {
      _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
      _config = runConfig?.DebugOptions?.InitConfig ?? new TableInitializationServiceConfig();
      _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
      _logger.LogInformation("TableInitializationService started...");

      // Ensure database and all mapped tables are created based on the current EF Core model.
      try
      {
        _logger.LogInformation("Ensuring database is created / migrated according to the current model...");
        // Prefer migrations in production; EnsureCreated is simple and will create whatever the model defines.
        await _dbContext.Database.EnsureCreatedAsync(ct);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error while ensuring database is created");
        // continue so we can attempt per-table checks
      }

      // initialize requested tables
      foreach (var table in _config.TablesToInit)
      {
        string tableName = EnumMapper.GetTable(table);
        _logger.LogInformation($"Initializing table: {tableName}...");
        switch (table)
        {
          case Table.DailyTF:
            await InitializeDailyTableAsync(tableName, ct);
            break;
          case Table.FifteenTF:
            await InitializeFifteenTableAsync(tableName, ct);
            break;
          case Table.HighLow:
            await InitializeHighLowTableAsync(tableName, ct);
            break;
          case Table.TrendLine:
            await InitializeTrendLineTableAsync(tableName, ct);
            break;
          case Table.Trade:
            await InitializeTradeTableAsync(ct);
            break;
          case Table.Simulation:
            await InitializeSimulationTableAsync(tableName, ct);
            break;
          default:
            _logger.LogWarning($"Initialization for table {table} is not implemented.");
            break;
        }
      }

      _logger.LogInformation("TableInitializationService completed.");
    }
    private async Task InitializeDailyTableAsync(string tableNameHint, CancellationToken ct)
    {
      // implement as needed
    }
    private async Task InitializeFifteenTableAsync(string tableNameHint, CancellationToken ct)
    {
      // implement as needed
    }

    /// <summary>
    /// Generic initializer that attempts to locate a matching DbSet on the DbContext and reports / seeds if necessary.
    /// It uses reflection so it doesn't need compile-time knowledge of the entity types.
    /// </summary>
    private async Task InitializeTrendLineTableAsync(string tableNameHint, CancellationToken ct)
    {
      _logger.LogInformation("Initializing TrendLine table");
      var tokens = AppConstants.StockLookUP.Values
                    .Select(ts => int.TryParse(ts, out var t) ? t : -1)
                    .Where(t => t != -1)
                    .Distinct()
                    .ToList();
      var timeframes = Utility.GetAllTimeframeStrings();
      Dictionary<int, string> trendlineLookup = new Dictionary<int, string>();
      foreach(var token in tokens)
      {
        trendlineLookup[token] = "";
        foreach(var tf in timeframes)
        {
          trendlineLookup[token] += tf + "h"+ tf + "l";
        }
      }
      // 1. Load all existing trendline rows
      var allRows = await _dbContext.Trendline.ToListAsync(ct);

      int count = 0;
      // 2. Update each row to EmptyTrendline values but keep the primary key (id)
      foreach (var row in allRows)
      {
        var empty = Trendline.EmptyTrendline();
          
          row.starttime = empty.starttime;
          row.endtime = empty.endtime;
          row.slope = empty.slope;
          row.intercept = empty.intercept;
          row.index = empty.index;
          row.index1 = empty.index1;
          row.index2 = empty.index2;
          row.connects = empty.connects;
          row.totalconnects = empty.totalconnects;
          trendlineLookup[row.token] = trendlineLookup[row.token].Replace(row.tf + row.hl, "");
      }

      // Save updates
      await _dbContext.SaveChangesAsync(ct);
      _logger.LogInformation($"Reset {allRows.Count} existing TrendLine rows to empty values.");
      // 3. Ensure every stock token has 1 row per timeframe
      // You MUST define your list of tokens and timeframes.
      // Example:
      
      count = 0;
      // initialize missing rows 
      foreach (var token in tokens)
      {
        foreach (var tf in timeframes)
        {
          var highRowNotExists = trendlineLookup[token].Contains(tf + "h");

          if (highRowNotExists)
          {
            count++;
            var empty = Trendline.EmptyHighTrendline();
            empty.token = token;
            empty.tf = tf;

            _dbContext.Trendline.Add(empty);
          }
          var lowRowNotExists = trendlineLookup[token].Contains(tf + "l");
          if (lowRowNotExists)
          {
            count++;
            var empty = Trendline.EmptyLowTrendline();
            empty.token = token;
            empty.tf = tf;

            _dbContext.Trendline.Add(empty);
          }
        }
      }
      await _dbContext.SaveChangesAsync(ct);
      _logger.LogInformation($"Inserted {count} missing TrendLine rows.");
      _logger.LogInformation("TrendLine table initialization completed.");
    }
    private async Task InitializeHighLowTableAsync(string tableNameHint, CancellationToken ct)
    {
      // implement as needed
    }
    private async Task InitializeSimulationTableAsync(string tableNameHint, CancellationToken ct)
    {
      // implement as needed
    }
    /// <summary>
    /// Special handling for the Trade table: if the DbSet isn't present we explicitly instruct the developer to add the entity
    /// (because EF Core cannot create a table that isn't defined in the model at runtime).
    /// </summary>
    private async Task InitializeTradeTableAsync(CancellationToken ct)
    {
      _logger.LogInformation("Initializing table TradeTable");

      // Try to locate a property likely representing trade data
      var ctxType = _dbContext.GetType();
      var prop = ctxType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .FirstOrDefault(p => p.Name.Equals("Trade", StringComparison.OrdinalIgnoreCase) || p.Name.IndexOf("Trade", StringComparison.OrdinalIgnoreCase) >= 0);

      if (prop == null)
      {
        _logger.LogWarning("DbContext does not expose a DbSet for trades (e.g. DbSet<Trade> Trades).\n" +
                           "EF Core cannot create a table for an entity that is not part of the model at runtime.\n" +
                           "To add a trade table, add a Trade entity, register it on the TradingDbContext, create and apply a migration (dotnet ef migrations add X; dotnet ef database update).\n" +
                           "If you want me to generate a sample Trade entity and the corresponding DbSet property, tell me the columns you need and I can prepare code for you.");
        return;
      }
    }
  }
}
