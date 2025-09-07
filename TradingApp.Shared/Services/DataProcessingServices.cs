using Microsoft.Extensions.Logging;
using TradingApp.Infrastructure.Data;

namespace TradingApp.Shared.Services
{

    public class DataProcessingService
    {
        private readonly TradingDbContext _db;
        private readonly ILogger<DataProcessingService> _logger;
        private readonly int _bulkThreshold;

        public DataProcessingService(TradingDbContext db, ILogger<DataProcessingService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task ExecuteAsync(CancellationToken ct)
        {
            // Map all incoming rows to entity list and do batch write per symbol (or global batches)
            // foreach (var fetched in fetchedList)
            // {
            //     ct.ThrowIfCancellationRequested();
            //     var rows = fetched.Rows.Select(r => new DailyTF
            //     {
            //         token = r.token,
            //         time = r.time,
            //         open = r.open,
            //         high = r.high,
            //         low = r.low,
            //         close = r.close,
            //         volume = r.volume
            //     }).ToList();

            //     if (rows.Count == 0) continue;

            //     _logger.LogInformation("Persisting {count} rows for {symbol} (token {token})", rows.Count, fetched.Symbol, fetched.Token);

            //     if (rows.Count <= _bulkThreshold)
            //     {
            //         await _db.DailyTF.AddRangeAsync(rows, ct);
            //         await _db.SaveChangesAsync(ct);
            //     }
            //     else
            //     {
            //         // Bulk insert - EFCore.BulkExtensions
            //         await _db.BulkInsertAsync(rows, cancellationToken: ct);
            //     }
            // }
        }
    }
}
