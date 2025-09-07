using Microsoft.Extensions.DependencyInjection;
using TradingApp.Core.Interfaces;

namespace TradingApp.Shared.ExternalApis
{
    public class MarketApiFactory : IMarketApiFactory
    {
        private readonly IServiceProvider _sp;
        public MarketApiFactory(IServiceProvider sp) => _sp = sp;

        public IMarketApiClient GetClient(string providerName)
        {
            // Map provider name to an implementation registered in DI
            // providerName should match how you registered clients in Program.cs
            return providerName switch
            {
                "Alpha" => _sp.GetRequiredService<AlphaVantageClient>(),
                "Dhan" => _sp.GetRequiredService<DhanClient>(),
                _ => throw new InvalidOperationException($"Unknown market provider '{providerName}'")
            };
        }
    }
}
