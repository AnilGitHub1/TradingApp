using System.Net.Http.Json;
using TradingApp.Core.Contracts;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure.Data;
using TradingApp.Shared.Constants;

namespace TradingApp.Shared.ExternalApis
{
  public class DhanClient<T> : IMarketApiClient<T> where T : Candle
  {
    private const string _url = "https://ticks.dhan.co/getDataH";
    private readonly DateTime DefaultStart = new(2008, 9, 11);
    private readonly DhanPayLoad _payLoad;
    private readonly TradingDbContext _db;
    private readonly HttpClient _http;
    private readonly IAppLogger<DhanClient<T>> _logger;

    public DhanClient(TradingDbContext db, HttpClient http, IAppLogger<DhanClient<T>> logger)
    {
      _db = db;
      _http = http;
      _logger = logger;
      _payLoad = new DhanPayLoad();
    }

    #region Public Fetch Methods
    public async Task<FetchResult<T>?> FetchAsync(string symbol, string timeFrame, CancellationToken ct)
      => await FetchInternalAsync([symbol], timeFrame, DefaultStart, ct);

    public async Task<FetchResult<T>?> FetchAsync(List<string> symbols, string timeFrame, CancellationToken ct)
      => await FetchInternalAsync(symbols, timeFrame, DefaultStart, ct);

    public async Task<FetchResult<T>?> FetchAsync(string timeFrame, CancellationToken ct)
      => await FetchInternalAsync([.. AppConstants.AllTokens.Values], timeFrame, DefaultStart, ct);

    public async Task<FetchResult<T>?> FetchAsync(string symbol, string timeFrame, DateTime start, CancellationToken ct)
      => await FetchInternalAsync([symbol], timeFrame, start, ct);

    public async Task<FetchResult<T>?> FetchAsync(List<string> symbols, string timeFrame, DateTime start, CancellationToken ct)
      => await FetchInternalAsync(symbols, timeFrame, start, ct);

    public async Task<FetchResult<T>?> FetchAsync(string timeFrame, DateTime start, CancellationToken ct)
      => await FetchInternalAsync([.. AppConstants.AllTokens.Values], timeFrame, start, ct);
    #endregion

    #region Private Helpers
    private async Task<FetchResult<T>?> FetchInternalAsync(List<string> symbols, string timeFrame, DateTime start, CancellationToken ct)
    {
      var combinedResult = new FetchResult<T>(new List<T>());

      foreach (var symbol in symbols)
      {
        try
        {
          // var token = AppConstants.AllTokens[symbol];
          SetPayload(symbol, start, timeFrame);
          int token;
          if (!AppConstants.StockLookUP.TryGetValue(symbol, out var tokenValue))
          {
            _logger.LogWarning("Symbol {Symbol} not found in token lookup.", symbol);
            continue;
          }
          if (!int.TryParse(tokenValue, out token))
          {
            _logger.LogWarning("Token {TokenValue} for symbol {Symbol} is not a valid integer.", tokenValue, symbol);
            continue;
          }
          var response = await _http.PostAsJsonAsync(_url, _payLoad, ct);

          if (!response.IsSuccessStatusCode)
          {
            _logger.LogError("Failed to fetch data for {Symbol}. Status: {StatusCode}", symbol, response.StatusCode);
            continue; // Skip this symbol, move to next
          }
          var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>(cancellationToken: ct);
          var candles = new List<T>();
          if (apiResponse != null)
          {
            candles = LoadCandles(apiResponse, token);
          }

          combinedResult.Candles.AddRange(candles);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error while fetching data for {Symbol}", symbol);
        }
      }

      return combinedResult.Candles.Count > 0 ? combinedResult : null;
    }

    private List<T> LoadCandles(ApiResponse apiResponse, int token)
    {
        var candles = new List<T>();
      if (apiResponse?.Success == true && apiResponse.Data != null)
      {
        var data = apiResponse.Data;

        for (int i = 0; i < data.t.Count; i++)
        {
          var args = new object[]
          {
            token,
            DateTimeOffset.FromUnixTimeSeconds(data.t[i]).DateTime,
            data.o[i],
            data.h[i],
            data.l[i],
            data.c[i],
            data.v[i]
          };
          candles.Add((T)Activator.CreateInstance(typeof(T), args));
        }
      }

      return candles;
    }

    private void SetPayload(string symbol, DateTime startTime, string timeFrame)
    {
      _payLoad.SYM = symbol;
      _payLoad.START_TIME = startTime.ToString("o"); // ISO 8601 format
      _payLoad.END_TIME = DateTime.Now.ToString("o");
      _payLoad.START = new DateTimeOffset(startTime).ToUnixTimeSeconds();
      _payLoad.END = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
      _payLoad.INTERVAL = timeFrame;
    }
    private class CandleData
    {
      public List<long> t { get; set; } = new();  // Unix timestamps
      public List<double> o { get; set; } = new();
      public List<double> h { get; set; } = new();
      public List<double> l { get; set; } = new();
      public List<double> c { get; set; } = new();
      public List<long> v { get; set; } = new();
    }

    private class ApiResponse
    {
      public bool Success { get; set; }
      public CandleData Data { get; set; } = new();
    }
    #endregion
  }
}
