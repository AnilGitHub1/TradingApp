using Microsoft.Extensions.DependencyInjection;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;

namespace TradingApp.Shared.ExternalApis
{
    public class MarketApiFactory<T> : IMarketApiFactory<T> where T : Candle
    {
        private readonly IServiceProvider _sp;
        public MarketApiFactory(IServiceProvider sp) => _sp = sp;

        public IMarketApiClient<T> GetClient(string providerName)
        {
            // Map provider name to an implementation registered in DI
            // providerName should match how you registered clients in Program.cs
            return providerName switch
            {
                "Dhan" => _sp.GetRequiredService<DhanClient<T>>(),
                _ => throw new InvalidOperationException($"Unknown market provider '{providerName}'")
            };
        }
    }
}
