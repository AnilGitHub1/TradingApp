using Microsoft.Extensions.Logging;
using TradingApp.Shared.ExternalApis;
using TradingApp.Shared.Options;

namespace TradingApp.Shared.Services
{
  public class DataFetchService
  {
    private readonly IMarketApiFactory _factory;
    private readonly ILogger<DataFetchService> _logger;
    private readonly FetchServiceConfig _cfg;
    private readonly List<string> _symbols;

    public DataFetchService(IMarketApiFactory factory, ILogger<DataFetchService> logger, RunConfig cfg)
    {
      _factory = factory;
      _logger = logger;
      _cfg = cfg.DebugOptions.FetchConfig;
      _symbols = cfg.DebugOptions.Symbols;
    }
    public async Task ExecuteAsync(CancellationToken ct)
    {

      var client = _factory.GetClient("Dhan");
      if (client == null)
      {
        _logger.LogError("No client registered for Dhan provider.");
      }
      else
      {
        switch (_cfg.fetchMode)
        {
          case Constants.FetchMode.ALL:
            await client.FetchAsync(_symbols, "1D", ct);
            break;
          case Constants.FetchMode.Latest:
            await client.FetchAsync(_symbols, "1D", GetLatestDateTime(), ct);
            break;
          default:
            break;
        }
      }
    }
    public static DateTime GetLatestDateTime()
    {
      return DateTime.Now;
    }
  }
}
