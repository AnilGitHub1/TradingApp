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
    private readonly string _url = "https://ticks.dhan.co/getDataH";
    private readonly DateTime DefaultStartFifteenTF = new DateTime(2024, 10, 8, 0, 0, 1);
    private readonly DateTime DefaultStartDailyTF = new DateTime(2018, 1, 1, 0, 0, 1);
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
      if(typeof(T) == typeof(FifteenTF))
      {
        _url = "https://ticks.dhan.co/getData";
      }
    }

    #region Public Fetch Methods
    public async Task<FetchResult<T>?> FetchAsync(string symbol, TimeFrame timeFrame, CancellationToken ct)
      => await FetchInternalAsync([symbol], timeFrame, timeFrame == TimeFrame.Day? DefaultStartDailyTF : DefaultStartFifteenTF, ct);

    public async Task<FetchResult<T>?> FetchAsync(List<string> symbols, TimeFrame timeFrame, CancellationToken ct)
      => await FetchInternalAsync(symbols, timeFrame, timeFrame == TimeFrame.Day? DefaultStartDailyTF : DefaultStartFifteenTF, ct);

    public async Task<FetchResult<T>?> FetchAsync(TimeFrame timeFrame, CancellationToken ct)
      => await FetchInternalAsync([.. AppConstants.AllTokens.Values], timeFrame, timeFrame == TimeFrame.Day? DefaultStartDailyTF : DefaultStartFifteenTF, ct);

    public async Task<FetchResult<T>?> FetchAsync(string symbol, TimeFrame timeFrame, DateTime start, CancellationToken ct)
      => await FetchInternalAsync([symbol], timeFrame, start, ct);

    public async Task<FetchResult<T>?> FetchAsync(List<string> symbols, TimeFrame timeFrame, DateTime start, CancellationToken ct)
      => await FetchInternalAsync(symbols, timeFrame, start, ct);

    public async Task<FetchResult<T>?> FetchAsync(TimeFrame timeFrame, DateTime start, CancellationToken ct)
      => await FetchInternalAsync([.. AppConstants.AllTokens.Values], timeFrame, start, ct);
    #endregion

    #region Private Helpers
    private async Task<FetchResult<T>?> FetchInternalAsync(List<string> symbols, TimeFrame timeFrame, DateTime start, CancellationToken ct)
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
            candles = LoadCandles(apiResponse, token, start);
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

    private List<T> LoadCandles(ApiResponse apiResponse, int token, DateTime startDateTime = default)
    {
        var candles = new List<T>();
        HashSet<long> existingTimes = new();
      if (apiResponse?.Success == true && apiResponse.Data != null && apiResponse.Data.t != null && apiResponse.Data.t.Count > 0)
      {
        var data = apiResponse.Data;

        for (int i = 0; i < data.t.Count; i++)
        {
          DateTime candleDateTime = DateTimeOffset.FromUnixTimeSeconds(data.t[i]).ToLocalTime().DateTime;
          if (startDateTime != default && candleDateTime <= startDateTime)
            continue;

          if (existingTimes.Contains(data.t[i]))
          {
            _logger.LogWarning("Duplicate timestamp {Timestamp} found for token {Token}. Skipping entry.", data.t[i], token);
            continue;
          }
          else
            existingTimes.Add(data.t[i]);
          var candle = new object[]
          {
            token,
            candleDateTime,
            data.o[i],
            data.h[i],
            data.l[i],
            data.c[i],
            (int)data.v[i]
          };
          candles.Add((T)Activator.CreateInstance(typeof(T), candle));
        }
      }

      return candles;
    }

    private void SetPayload(string symbol, DateTime startTime, TimeFrame timeFrame)
    {
      var startOffset = new DateTimeOffset(startTime, TimeZoneInfo.Local.GetUtcOffset(startTime));
      _payLoad.SYM = symbol;
      _payLoad.START_TIME = timeFrame == TimeFrame.FifteenMinute ?
        ToJsLikeString(new DateTime(startTime.Year, startTime.Month, startTime.Day, 0, 0, 1))
       :startTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"); // ISO 8601 format
      _payLoad.END_TIME = timeFrame == TimeFrame.FifteenMinute ?
        ToJsLikeString(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1, 0, 0, 1))
       :DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
      _payLoad.START = startOffset.ToUnixTimeSeconds();
      _payLoad.END = DateTimeOffset.Now.ToUnixTimeSeconds();
      _payLoad.INTERVAL = GetIntervalString(timeFrame);
    }

    string ToJsLikeString(DateTime dt)
    {
      return dt.ToString("ddd MMM dd yyyy HH:mm:ss 'GMT'zzz '(India Standard Time)'", System.Globalization.CultureInfo.InvariantCulture);
    }

    private string GetIntervalString(TimeFrame timeFrame)
    {
      return timeFrame switch
      {
        TimeFrame.FifteenMinute => "15",
        TimeFrame.Day => "D",
        _ => throw new ArgumentException($"Unsupported time frame: {timeFrame}"),
      };
    }
    private class CandleData
    {
      public List<long> t { get; set; } = new();  // Unix timestamps
      public List<double> o { get; set; } = new();
      public List<double> h { get; set; } = new();
      public List<double> l { get; set; } = new();
      public List<double> c { get; set; } = new();
      public List<double> v { get; set; } = new();
    }

    private class ApiResponse
    {
      public bool Success { get; set; }
      public CandleData Data { get; set; } = new();
    }
    #endregion
  }
}
