using System.Net.Http;
using System.Net.Http.Json;
using TradingApp.Core.Contracts;
using TradingApp.Core.DTOs;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure.Data;
using TradingApp.Shared.Constants;

namespace TradingApp.Shared.ExternalApis
{
  public class DhanClient : IMarketApiClient
  {
    private const string _url = "https://ticks.dhan.co/getDataH";
    private readonly DateTime DefaultStart = new(2008, 9, 11);
    private readonly DhanPayLoad _payLoad;
    private readonly TradingDbContext _db;
    private readonly HttpClient _http;
    private readonly IAppLogger<DhanClient> _logger;

    public DhanClient(TradingDbContext db, HttpClient http, IAppLogger<DhanClient> logger)
    {
      _db = db;
      _http = http;
      _logger = logger;
      _payLoad = new DhanPayLoad();
    }

    #region Public Fetch Methods
    public async Task<FetchResult?> FetchAsync(string symbol, string timeFrame, CancellationToken ct)
      => await FetchInternalAsync(new List<string>{string.Empty}, timeFrame, DefaultStart, DateTime.Now, ct);

    public async Task<FetchResult?> FetchAsync(List<string> symbols, string timeFrame, CancellationToken ct)
      => await FetchInternalAsync(symbols, timeFrame, DefaultStart, DateTime.Now, ct);

    public async Task<FetchResult?> FetchAsync(string timeFrame, CancellationToken ct)
      => await FetchInternalAsync([.. AppConstants.AllTokens.Keys], timeFrame, DefaultStart, DateTime.Now, ct);

    public async Task<FetchResult?> FetchAsync(string symbol, string timeFrame, DateTime start, CancellationToken ct)
      => await FetchInternalAsync([symbol], timeFrame, start, DateTime.Now, ct);

    public async Task<FetchResult?> FetchAsync(List<string> symbols, string timeFrame, DateTime start, CancellationToken ct)
      => await FetchInternalAsync(symbols, timeFrame, start, DateTime.Now, ct);

    public async Task<FetchResult?> FetchAsync(string timeFrame, DateTime start, CancellationToken ct)
      => await FetchInternalAsync([.. AppConstants.AllTokens.Keys], timeFrame, start, DateTime.Now, ct);
    #endregion

    #region Private Helpers
    private async Task<FetchResult?> FetchInternalAsync(List<string> symbols, string timeFrame, DateTime start, DateTime end, CancellationToken ct)
    {
      var combinedResult = new FetchResult { Candles = new List<Candle>() };

      foreach (var symbol in symbols)
      {
        try
        {
          SetPayload(symbol, start, end, timeFrame);
          var response = await _http.PostAsJsonAsync(_url, _payLoad, ct);

          if (!response.IsSuccessStatusCode)
          {                      
            _logger.LogError("Failed to fetch data for {Symbol}. Status: {StatusCode}", symbol, response.StatusCode);
            continue; // Skip this symbol, move to next
          }

          var result = await response.Content.ReadFromJsonAsync<FetchResult>(cancellationToken: ct);
          if (result?.Candles != null)
          {
            combinedResult.Candles.AddRange(result.Candles);
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error while fetching data for {Symbol}", symbol);
        }
      }

      return combinedResult.Candles.Count > 0 ? combinedResult : null;
    }

    private void SetPayload(string symbol, DateTime startTime, DateTime endTime, string timeFrame)
    {
      _payLoad.SYM = symbol;
      _payLoad.START_TIME = startTime.ToString("o"); // ISO 8601 format
      _payLoad.END_TIME = endTime.ToString("o");
      _payLoad.START = new DateTimeOffset(startTime).ToUnixTimeSeconds();
      _payLoad.END = new DateTimeOffset(endTime).ToUnixTimeSeconds();
      _payLoad.INTERVAL = timeFrame;
    }
    #endregion
  }
}
