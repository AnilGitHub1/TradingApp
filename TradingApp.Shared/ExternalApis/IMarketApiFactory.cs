using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;

namespace TradingApp.Shared.ExternalApis
{
  public interface IMarketApiFactory<T> where T : Candle
  {
    IMarketApiClient<T> GetClient(string providerName);
  }
}
