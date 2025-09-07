using TradingApp.Core.Interfaces;

namespace TradingApp.Shared.ExternalApis
{
    public interface IMarketApiFactory
    {
        IMarketApiClient GetClient(string providerName);
    }
}
