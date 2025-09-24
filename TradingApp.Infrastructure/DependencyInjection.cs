using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure.Data;
using TradingApp.Infrastructure.Repositories;

namespace TradingApp.Infrastructure
{
  public static class DependencyInjection
  {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString,
    bool addGenericCandleRepository = false)
    {
      services.AddDbContext<TradingDbContext>(options =>
          options.UseNpgsql(connectionString));
      if (addGenericCandleRepository)
      {
        services.AddScoped(typeof(ICandleRepository<>), typeof(CandleRepository<>));
      }
      services.AddScoped<IDailyTFRepository, DailyTFRepository>();
      services.AddScoped<IFifteenTFRepository, FifteenTFRepository>();
      services.AddScoped<IHighLowRepository, HighLowRepository>();
      services.AddScoped<ITrendlineRepository, TrendlineRepository>();

      return services;
    }
  }
}
