using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using TradingApp.Core.Contracts;
using TradingApp.Core.DTOs;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;

namespace TradingApp.Shared.ExternalApis
{
    /// <summary>
    /// Example provider implementation. Replace mapping logic with your actual API response format.
    /// </summary>
    public class AlphaVantageClient : IMarketApiClient
    {
        private readonly HttpClient _http;
        private readonly IAppLogger<AlphaVantageClient> _logger;

        public AlphaVantageClient(HttpClient http, IAppLogger<AlphaVantageClient> logger)
        {
            _http = http;
            _logger = logger;
        }
        public async Task<FetchResult?> FetchAsync(string symbol, string timeframe, CancellationToken ct)
        {
            // This is an example URL pattern; replace with your actual endpoint/parameters.
            var url = $"query?symbol={Uri.EscapeDataString(symbol)}&tf={Uri.EscapeDataString(timeframe)}";
            try
            {
                _logger.LogInformation("Calling Alpha provider for {symbol} {tf}", symbol, timeframe);
                var resp = await _http.GetAsync(url, ct);
                resp.EnsureSuccessStatusCode();

                // Example: assume API returns JSON array of candles.
                var json = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                // Replace the parse below with your real JSON shape
                var token = doc.RootElement.GetProperty("token").GetInt32();
                var list = new List<Candle>();
                foreach (var el in doc.RootElement.GetProperty("candles").EnumerateArray())
                {
                    var time = el.GetProperty("timestamp").GetDateTime();
                    var open = el.GetProperty("open").GetDouble();
                    var high = el.GetProperty("high").GetDouble();
                    var low = el.GetProperty("low").GetDouble();
                    var close = el.GetProperty("close").GetDouble();
                    var volume = el.GetProperty("volume").GetDouble();
                    list.Add(new Candle(
                        token,
                        time = el.GetProperty("timestamp").GetDateTime(),
                        open = el.GetProperty("open").GetDouble(),
                        high = el.GetProperty("timestamp").GetDouble(),
                        low = el.GetProperty("timestamp").GetDouble(),
                        close = el.GetProperty("timestamp").GetDouble(),
                        volume = el.GetProperty("timestamp").GetDouble()));
                }

                return new FetchResult(symbol, token, list);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Alpha provider failed for {symbol}", symbol);
                return null;
            }
        }
        public async Task<FetchResult?> FetchAsync(string[] symbols, string timeframe, CancellationToken ct)
        {
            var symbol = symbols[0];
            // This is an example URL pattern; replace with your actual endpoint/parameters.
            var url = $"query?symbol={Uri.EscapeDataString(symbol)}&tf={Uri.EscapeDataString(timeframe)}";
            try
            {
                _logger.LogInformation("Calling Alpha provider for {symbol} {tf}", symbol, timeframe);
                var resp = await _http.GetAsync(url, ct);
                resp.EnsureSuccessStatusCode();

                // Example: assume API returns JSON array of candles.
                var json = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                // Replace the parse below with your real JSON shape
                var token = doc.RootElement.GetProperty("token").GetInt32();
                var list = new List<Candle>();
                foreach (var el in doc.RootElement.GetProperty("candles").EnumerateArray())
                {
                    var time = el.GetProperty("timestamp").GetDateTime();
                    var open = el.GetProperty("open").GetDouble();
                    var high = el.GetProperty("high").GetDouble();
                    var low = el.GetProperty("low").GetDouble();
                    var close = el.GetProperty("close").GetDouble();
                    var volume = el.GetProperty("volume").GetDouble();
                    list.Add(new Candle(
                        token,
                        time = el.GetProperty("timestamp").GetDateTime(),
                        open = el.GetProperty("open").GetDouble(),
                        high = el.GetProperty("timestamp").GetDouble(),
                        low = el.GetProperty("timestamp").GetDouble(),
                        close = el.GetProperty("timestamp").GetDouble(),
                        volume = el.GetProperty("timestamp").GetDouble()));
                }

                return new FetchResult(symbol, token, list);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Alpha provider failed for {symbol}", symbol);
                return null;
            }
        }
        public async Task<FetchResult?> FetchAsync(string symbol, string timeframe, DateTime start, CancellationToken ct)
        {
            // This is an example URL pattern; replace with your actual endpoint/parameters.
            var url = $"query?symbol={Uri.EscapeDataString(symbol)}&tf={Uri.EscapeDataString(timeframe)}";
            try
            {
                _logger.LogInformation("Calling Alpha provider for {symbol} {tf}", symbol, timeframe);
                var resp = await _http.GetAsync(url, ct);
                resp.EnsureSuccessStatusCode();

                // Example: assume API returns JSON array of candles.
                var json = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                // Replace the parse below with your real JSON shape
                var token = doc.RootElement.GetProperty("token").GetInt32();
                var list = new List<Candle>();
                foreach (var el in doc.RootElement.GetProperty("candles").EnumerateArray())
                {
                    var time = el.GetProperty("timestamp").GetDateTime();
                    var open = el.GetProperty("open").GetDouble();
                    var high = el.GetProperty("high").GetDouble();
                    var low = el.GetProperty("low").GetDouble();
                    var close = el.GetProperty("close").GetDouble();
                    var volume = el.GetProperty("volume").GetDouble();
                    list.Add(new Candle(
                        token,
                        time = el.GetProperty("timestamp").GetDateTime(),
                        open = el.GetProperty("open").GetDouble(),
                        high = el.GetProperty("timestamp").GetDouble(),
                        low = el.GetProperty("timestamp").GetDouble(),
                        close = el.GetProperty("timestamp").GetDouble(),
                        volume = el.GetProperty("timestamp").GetDouble()));
                }

                return new FetchResult(symbol, token, list);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Alpha provider failed for {symbol}", symbol);
                return null;
            }
        }
        public async Task<FetchResult?> FetchAsync(string[] symbols, string timeframe, DateTime start, CancellationToken ct)
        {
            var symbol = symbols[0];
            // This is an example URL pattern; replace with your actual endpoint/parameters.
            var url = $"query?symbol={Uri.EscapeDataString(symbol)}&tf={Uri.EscapeDataString(timeframe)}";
            try
            {
                _logger.LogInformation("Calling Alpha provider for {symbol} {tf}", symbol, timeframe);
                var resp = await _http.GetAsync(url, ct);
                resp.EnsureSuccessStatusCode();

                // Example: assume API returns JSON array of candles.
                var json = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                // Replace the parse below with your real JSON shape
                var token = doc.RootElement.GetProperty("token").GetInt32();
                var list = new List<Candle>();
                foreach (var el in doc.RootElement.GetProperty("candles").EnumerateArray())
                {
                    var time = el.GetProperty("timestamp").GetDateTime();
                    var open = el.GetProperty("open").GetDouble();
                    var high = el.GetProperty("high").GetDouble();
                    var low = el.GetProperty("low").GetDouble();
                    var close = el.GetProperty("close").GetDouble();
                    var volume = el.GetProperty("volume").GetDouble();
                    list.Add(new Candle(
                        token,
                        time = el.GetProperty("timestamp").GetDateTime(),
                        open = el.GetProperty("open").GetDouble(),
                        high = el.GetProperty("timestamp").GetDouble(),
                        low = el.GetProperty("timestamp").GetDouble(),
                        close = el.GetProperty("timestamp").GetDouble(),
                        volume = el.GetProperty("timestamp").GetDouble()));
                }

                return new FetchResult(symbol, token, list);
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
