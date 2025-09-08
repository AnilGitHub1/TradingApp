using Microsoft.Extensions.Logging;
using TradingApp.Infrastructure.Data;
using TradingApp.Core.Contracts;
using TradingApp.Core.Interfaces;

namespace TradingApp.Shared.Services
{

    public class AnalysisService : IService
    {
        private readonly TradingDbContext _db;
        private readonly ILogger<AnalysisService> _logger;

        public AnalysisService(TradingDbContext db, ILogger<AnalysisService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task ExecuteAsync(CancellationToken ct)
        {
            // _logger.LogInformation("Analyzing {symbol}", fetched.Symbol);

            // // Example: simple moving average over last N candles (replace with your logic)
            // var lastN = 20;
            // var rows = await _db.DailyTF
            //     .AsNoTracking()
            //     .Where(r => r.token == fetched.Token)
            //     .OrderByDescending(r => r.time)
            //     .Take(lastN)
            //     .ToListAsync(ct);

            // if (rows.Count == 0)
            // {
            //     _logger.LogWarning("No data to analyze for {symbol}", fetched.Symbol);
            //     return;
            // }

            // var sma = rows.Average(r => r.close);

            // Store the result into AnalysisResults table
            // var result = new AnalysisResult
            // {
            //     Token = fetched.Token,
            //     Symbol = fetched.Symbol,
            //     CalculatedAt = DateTime.UtcNow,
            //     Metric = (double)sma,
            //     MetricName = "SMA20"
            // };

            // _db.Add(result);
            // await _db.SaveChangesAsync(ct);

            // _logger.LogInformation("Stored analysis for {symbol} SMA20={sma}", fetched.Symbol, sma);
        }
    }
}