using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradingApp.Core.Interfaces;
using TradingApp.Core.Services;
using TradingApp.Infrastructure.Data;
using TradingApp.Infrastructure.Repositories;

namespace TradingApp.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<TradingDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IDailyTFRepository, DailyTFRepository>();
            services.AddScoped<IFifteenTFRepository, FifteenTFRepository>();
            services.AddScoped<IHighLowRepository, HighLowRepository>();
            services.AddScoped<IDailyTFService, DailyTFService>();

            return services;
        }
    }
}
