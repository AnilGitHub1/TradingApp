using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Xml.Serialization;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure;
using TradingApp.Shared.Logging;
using TradingApp.Shared.Services;
using TradingApp.Processor.Workers;
using TradingApp.Shared.Options;
using TradingApp.Shared.ExternalApis;
using TradingApp.Core.Entities;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        var env = ctx.HostingEnvironment;
        var basePath = Path.Combine(env.ContentRootPath);  // Ensures root project folder
        Console.WriteLine(basePath);
        cfg.SetBasePath(basePath);
        cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        cfg.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
        cfg.AddEnvironmentVariables();
    })
    .ConfigureServices((ctx, services) =>
    {
        var config = ctx.Configuration;

        // Load runConfig.xml to decide mode
        RunConfig runConfig = LoadRunConfig(config["PathConfig:InputConfigPath"] ?? "");

        // Infrastructure registration (assumes AddInfrastructure exists and registers TradingDbContext)
        services.AddInfrastructure(config.GetConnectionString("DefaultConnection")!);

        // Processor services
        services.AddHttpClient(); // default factory

        // Logging adapter
        AddSingletons(services, runConfig);
        AddHttpClients(services,config);
        AddTransients(services);
        AddScopes(services);

        // Decide which worker to run based on XML config

        if (runConfig.Mode.Equals("Background", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHostedService<MarketCloseWorker>();
        }
        else if (runConfig.Mode.Equals("Debug", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHostedService<DebugWorker>();
        }
        else
        {
            throw new InvalidOperationException($"Unknown mode: {runConfig.Mode}");
        }
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

await host.RunAsync();

// Helper method for XML deserialization
static RunConfig LoadRunConfig(string path)
{
    if (!File.Exists(path))
    {
        // default to Background if no XML found
        return new RunConfig { Mode = "Background" };
    }

    var serializer = new XmlSerializer(typeof(RunConfig));
    using var stream = File.OpenRead(path);
    return (RunConfig)serializer.Deserialize(stream)!;
}

static void AddHttpClients(IServiceCollection services, IConfiguration config)
{
    // External API providers and factory
    services.AddHttpClient<AlphaVantageClient<DailyTF>>(client =>
    {
        client.BaseAddress = new Uri(
            config["ExternalApi:AlphaBaseUrl"] 
            ?? config["ExternalApi:BaseUrl"] 
            ?? "https://api.example.com/");
    });
    services.AddHttpClient<AlphaVantageClient<FifteenTF>>(client =>
    {
        client.BaseAddress = new Uri(
            config["ExternalApi:AlphaBaseUrl"] 
            ?? config["ExternalApi:BaseUrl"] 
            ?? "https://api.example.com/");
    });
    services.AddHttpClient<DhanClient<DailyTF>>((sp, client) =>
    {
        var spConfig = sp.GetRequiredService<IConfiguration>();
        var section = spConfig.GetSection("DhanMarketDataProvider");

        // Set BaseAddress
        var baseUrl = section["BaseUrl"] ?? "https://tv-web.dhan.co";
        client.BaseAddress = new Uri(baseUrl);

        // Add default headers from config
        var headers = section.GetSection("Headers").GetChildren();
        foreach (var header in headers)
        {
            // Avoid duplicate assignment if header already exists
            if (!client.DefaultRequestHeaders.Contains(header.Key))
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            AutomaticDecompression = 
                System.Net.DecompressionMethods.GZip |
                System.Net.DecompressionMethods.Deflate |
                System.Net.DecompressionMethods.Brotli
        };
    });

    services.AddHttpClient<DhanClient<FifteenTF>>((sp, client) =>
    {
        var spConfig = sp.GetRequiredService<IConfiguration>();
        var section = spConfig.GetSection("DhanMarketDataProvider");

        // Set BaseAddress
        var baseUrl = section["BaseUrl"] ?? "https://tv-web.dhan.co";
        client.BaseAddress = new Uri(baseUrl);

        // Add default headers from config
        var headers = section.GetSection("Headers").GetChildren();
        foreach (var header in headers)
        {
            // Avoid duplicate assignment if header already exists
            if (!client.DefaultRequestHeaders.Contains(header.Key))
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            AutomaticDecompression = 
                System.Net.DecompressionMethods.GZip |
                System.Net.DecompressionMethods.Deflate |
                System.Net.DecompressionMethods.Brotli
        };
    });
}

static void AddSingletons(IServiceCollection services, RunConfig runConfig)
{
    services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
    services.AddSingleton<IMarketApiFactory<DailyTF>, MarketApiFactory<DailyTF>>();
    services.AddSingleton<IMarketApiFactory<FifteenTF>, MarketApiFactory<FifteenTF>>();
    services.AddSingleton(runConfig);
}

static void AddTransients(IServiceCollection services) {    
    services.AddTransient<AlphaVantageClient<DailyTF>>();
    services.AddTransient<DhanClient<DailyTF>>(); 
    services.AddTransient<AlphaVantageClient<FifteenTF>>();
    services.AddTransient<DhanClient<FifteenTF>>();
}

static void AddScopes(IServiceCollection services)
{
    services.AddScoped<DataFetchService<DailyTF>>();
    services.AddScoped<DataFetchService<FifteenTF>>();
    services.AddScoped<DataProcessingService>();
    services.AddScoped<AnalysisService>();
}
