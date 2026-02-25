using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using TradingApp.Core.Contracts;
using TradingApp.Core.DTOs;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Shared.Constants;

namespace TradingApp.Shared.ExternalApis
{
  /// <summary>
  /// Example provider implementation. Replace mapping logic with your actual API response format.
  /// </summary>
  public class AlphaVantageClient<T> where T:Candle
  {
    private readonly HttpClient _http;
    private readonly IAppLogger<AlphaVantageClient<T>> _logger;

    public AlphaVantageClient(HttpClient http, IAppLogger<AlphaVantageClient<T>> logger)
    {
      _http = http;
      _logger = logger;
    }
    public async Task<FetchResult<T>?> FetchAsync(string symbol, TimeFrame timeFrame, CancellationToken ct)
    {
      // This is an example URL pattern; replace with your actual endpoint/parameters.
      var url = $"query?symbol={Uri.EscapeDataString(symbol)}&tf={Uri.EscapeDataString(timeFrame.ToString())}";
      try
      {
        _logger.LogInformation("Calling Alpha provider for {symbol} {tf}", symbol, timeFrame.ToString());
        var resp = await _http.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();

        // Example: assume API returns JSON array of candles.
        var json = await resp.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        // Replace the parse below with your real JSON shape
        var token = doc.RootElement.GetProperty("token").GetInt32();
        var list = new List<T>();
        foreach (var el in doc.RootElement.GetProperty("candles").EnumerateArray())
        {
          var time = el.GetProperty("timestamp").GetDateTime();
          var open = el.GetProperty("open").GetDouble();
          var high = el.GetProperty("high").GetDouble();
          var low = el.GetProperty("low").GetDouble();
          var close = el.GetProperty("close").GetDouble();
          var volume = el.GetProperty("volume").GetDouble();
          var candle = new object[]
          {
            token,
            time,
            open,
            high,
            low,
            close,
            (int)volume
          };
          var obj = Activator.CreateInstance(typeof(T), candle);

          if (obj is T item)
          {
            list.Add(item);
          }
          else
          {
            throw new InvalidOperationException($"Type {typeof(T)} must have a constructor accepting candle data.");
          }
        }

        return new FetchResult<T>(list);
      }
      catch (OperationCanceledException) { throw; }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Alpha provider failed for {symbol}", symbol);
        return null;
      }
    }
    public async Task<FetchResult<T>?> FetchAsync(List<string> symbols, TimeFrame timeFrame, CancellationToken ct)
    {
      var symbol = symbols[0];
      // This is an example URL pattern; replace with your actual endpoint/parameters.
      var url = $"query?symbol={Uri.EscapeDataString(symbol)}&tf={Uri.EscapeDataString(timeFrame.ToString())}";
      try
      {
        _logger.LogInformation("Calling Alpha provider for {symbol} {tf}", symbol, timeFrame.ToString());
        var resp = await _http.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();

        // Example: assume API returns JSON array of candles.
        var json = await resp.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        // Replace the parse below with your real JSON shape
        var token = doc.RootElement.GetProperty("token").GetInt32();
        var list = new List<T>();
        foreach (var el in doc.RootElement.GetProperty("candles").EnumerateArray())
        {
          var time = el.GetProperty("timestamp").GetDateTime();
          var open = el.GetProperty("open").GetDouble();
          var high = el.GetProperty("high").GetDouble();
          var low = el.GetProperty("low").GetDouble();
          var close = el.GetProperty("close").GetDouble();
          var volume = el.GetProperty("volume").GetDouble();
          var candle = new object[]
          {
            token,
            time,
            open,
            high,
            low,
            close,
            (int)volume
          };
          var obj = Activator.CreateInstance(typeof(T), candle);

          if (obj is T item)
          {
            list.Add(item);
          }
          else
          {
            throw new InvalidOperationException($"Type {typeof(T)} must have a constructor accepting candle data.");
          }
        }

        return new FetchResult<T>(list);
      }
      catch (OperationCanceledException) { throw; }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Alpha provider failed for {symbol}", symbol);
        return null;
      }
    }
    public async Task<FetchResult<T>?> FetchAsync(string symbol, TimeFrame timeFrame, DateTime start, CancellationToken ct)
    {
      // This is an example URL pattern; replace with your actual endpoint/parameters.
      var url = $"query?symbol={Uri.EscapeDataString(symbol)}&tf={Uri.EscapeDataString(timeFrame.ToString())}";
      try
      {
        _logger.LogInformation("Calling Alpha provider for {symbol} {tf}", symbol, timeFrame.ToString());
        var resp = await _http.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();

        // Example: assume API returns JSON array of candles.
        var json = await resp.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        // Replace the parse below with your real JSON shape
        var token = doc.RootElement.GetProperty("token").GetInt32();
        var list = new List<T>();
        foreach (var el in doc.RootElement.GetProperty("candles").EnumerateArray())
        {
          var time = el.GetProperty("timestamp").GetDateTime();
          var open = el.GetProperty("open").GetDouble();
          var high = el.GetProperty("high").GetDouble();
          var low = el.GetProperty("low").GetDouble();
          var close = el.GetProperty("close").GetDouble();
          var volume = el.GetProperty("volume").GetDouble();
          var candle = new object[]
          {
            token,
            time,
            open,
            high,
            low,
            close,
            (int)volume
          };
          var obj = Activator.CreateInstance(typeof(T), candle);

          if (obj is T item)
          {
            list.Add(item);
          }
          else
          {
            throw new InvalidOperationException($"Type {typeof(T)} must have a constructor accepting candle data.");
          }
        }

        return new FetchResult<T>(list);
      }
      catch (OperationCanceledException) { throw; }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Alpha provider failed for {symbol}", symbol);
        return null;
      }
    }
    public async Task<FetchResult<T>?> FetchAsync(List<string> symbols, TimeFrame timeFrame, DateTime start, CancellationToken ct)
    {
      var symbol = symbols[0];
      // This is an example URL pattern; replace with your actual endpoint/parameters.
      var url = $"query?symbol={Uri.EscapeDataString(symbol)}&tf={Uri.EscapeDataString(timeFrame.ToString())}";
      try
      {
        _logger.LogInformation("Calling Alpha provider for {symbol} {tf}", symbol, timeFrame.ToString());
        var resp = await _http.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();

        // Example: assume API returns JSON array of candles.
        var json = await resp.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        // Replace the parse below with your real JSON shape
        var token = doc.RootElement.GetProperty("token").GetInt32();
        var list = new List<T>();
        foreach (var el in doc.RootElement.GetProperty("candles").EnumerateArray())
        {
          var time = el.GetProperty("timestamp").GetDateTime();
          var open = el.GetProperty("open").GetDouble();
          var high = el.GetProperty("high").GetDouble();
          var low = el.GetProperty("low").GetDouble();
          var close = el.GetProperty("close").GetDouble();
          var volume = el.GetProperty("volume").GetDouble();
          var candle = new object[]
          {
            token,
            time,
            open,
            high,
            low,
            close,
            (int)volume
          };
          var obj = Activator.CreateInstance(typeof(T), candle);

          if (obj is T item)
          {
            list.Add(item);
          }
          else
          {
            throw new InvalidOperationException($"Type {typeof(T)} must have a constructor accepting candle data.");
          }
        }

        return new FetchResult<T>(list);
      }
      catch (OperationCanceledException) { throw; }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Alpha provider failed for {symbol}", symbol);
        return null;
      }
    }
    public async Task<FetchResult<T>?> FetchAsync(TimeFrame timeFrame, CancellationToken ct)
        {
            var symbol = "symbols[0]";
            // This is an example URL pattern; replace with your actual endpoint/parameters.
            var url = $"query?symbol={Uri.EscapeDataString(symbol)}&tf={Uri.EscapeDataString(timeFrame.ToString())}";
            try
            {
                _logger.LogInformation("Calling Alpha provider for {symbol} {tf}", symbol, timeFrame.ToString());
                var resp = await _http.GetAsync(url, ct);
                resp.EnsureSuccessStatusCode();

                // Example: assume API returns JSON array of candles.
                var json = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                // Replace the parse below with your real JSON shape
                var token = doc.RootElement.GetProperty("token").GetInt32();
                var list = new List<T>();
                foreach (var el in doc.RootElement.GetProperty("candles").EnumerateArray())
                {
                  var time = el.GetProperty("timestamp").GetDateTime();
                  var open = el.GetProperty("open").GetDouble();
                  var high = el.GetProperty("high").GetDouble();
                  var low = el.GetProperty("low").GetDouble();
                  var close = el.GetProperty("close").GetDouble();
                  var volume = el.GetProperty("volume").GetDouble();
                  var candle = new object[]
          {
            token,
            time,
            open,
            high,
            low,
            close,
            (int)volume
          };
          var obj = Activator.CreateInstance(typeof(T), candle);

          if (obj is T item)
          {
            list.Add(item);
          }
          else
          {
            throw new InvalidOperationException($"Type {typeof(T)} must have a constructor accepting candle data.");
          }
                }

                return new FetchResult<T>(list);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Alpha provider failed for {symbol}", symbol);
                return null;
            }
        }
    public async Task<FetchResult<T>?> FetchAsync(TimeFrame timeFrame, DateTime start, CancellationToken ct)
        {
            var symbol = "symbols[0]";
            // This is an example URL pattern; replace with your actual endpoint/parameters.
            var url = $"query?symbol={Uri.EscapeDataString(symbol)}&tf={Uri.EscapeDataString(timeFrame.ToString())}";
            try
            {
                _logger.LogInformation("Calling Alpha provider for {symbol} {tf}", symbol, timeFrame.ToString());
                var resp = await _http.GetAsync(url, ct);
                resp.EnsureSuccessStatusCode();

                // Example: assume API returns JSON array of candles.
                var json = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                // Replace the parse below with your real JSON shape
                var token = doc.RootElement.GetProperty("token").GetInt32();
                var list = new List<T>();
                foreach (var el in doc.RootElement.GetProperty("candles").EnumerateArray())
                {
                  var time = el.GetProperty("timestamp").GetDateTime();
                  var open = el.GetProperty("open").GetDouble();
                  var high = el.GetProperty("high").GetDouble();
                  var low = el.GetProperty("low").GetDouble();
                  var close = el.GetProperty("close").GetDouble();
                  var volume = el.GetProperty("volume").GetDouble();
                  var candle = new object[]
          {
            token,
            time,
            open,
            high,
            low,
            close,
            (int)volume
          };
          var obj = Activator.CreateInstance(typeof(T), candle);

          if (obj is T item)
          {
            list.Add(item);
          }
          else
          {
            throw new InvalidOperationException($"Type {typeof(T)} must have a constructor accepting candle data.");
          }
                }

                return new FetchResult<T>(list);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Alpha provider failed for {symbol}", symbol);
                return null;
            }
        }
    
  }
}
