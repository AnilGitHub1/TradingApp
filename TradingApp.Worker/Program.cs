using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure;
using TradingApp.Shared.Logging;
using TradingApp.Shared.Services;
using TradingApp.Processor.Workers;
using TradingApp.Shared.Options;
using TradingApp.Shared.ExternalApis;
using TradingApp.Core.Entities;
using TradingApp.Shared.Constants;
using Microsoft.Extensions.Options;
using System.Xml.Serialization;

var host = Host.CreateDefaultBuilder(args)
  .ConfigureAppConfiguration((ctx, cfg) =>
  {
    var env = ctx.HostingEnvironment;
    cfg.SetBasePath(env.ContentRootPath);
    cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    cfg.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
    cfg.AddEnvironmentVariables();
  })
  .UseSerilog((ctx, services, loggerConfig) =>
  {
    var logPath = ctx.Configuration["PathConfig:LoggingPath"] ?? "Logs/log-.txt";

    loggerConfig
      .ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console(
        outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}"
      )
      .WriteTo.File(
        path: logPath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 10,
        shared: true,
        outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}");
  })
  .ConfigureServices((ctx, services) =>
  {
    var config = ctx.Configuration;
    Console.WriteLine($"Environment: {ctx.HostingEnvironment.EnvironmentName}");

    RunConfig runConfig = LoadRunConfig(config["PathConfig:InputConfigPath"] ?? "");
    Console.WriteLine($"Running in {runConfig.Mode} mode.");

    // Infrastructure
    services.AddInfrastructure(config.GetConnectionString("DefaultConnection")!, true);

    // Common services
    services.AddHttpClient();
    AddSingletons(services, runConfig);
    AddHttpClients(services, config);
    AddScopes(services);

    // Workers
    if (runConfig.Mode.Equals("Background", StringComparison.OrdinalIgnoreCase))
      services.AddHostedService<MarketCloseWorker>();
    else if (runConfig.Mode.Equals("Debug", StringComparison.OrdinalIgnoreCase))
      services.AddHostedService<DebugWorker>();
    else
      throw new InvalidOperationException($"Unknown mode: {runConfig.Mode}");
  })
  .Build();

await host.RunAsync();

// ----------------- Helper methods -----------------

static RunConfig LoadRunConfig(string path)
{
  if (!File.Exists(path))
    return new RunConfig { Mode = "Background" };

  var serializer = new XmlSerializer(typeof(RunConfig));
  using var stream = File.OpenRead(path);
  var cfg = (RunConfig)serializer.Deserialize(stream)!;
  if (cfg.DebugOptions.Symbols == null || cfg.DebugOptions.Symbols.Count == 0)
    cfg.DebugOptions.Symbols = [..AppConstants.AllTokens.Values];
  return cfg;
}

static void AddHttpClients(IServiceCollection services, IConfiguration config)
{
  services.Configure<DhanConfig>(config.GetSection("DhanMarketDataProvider"));

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

  AddDhanClient<DailyTF>(services);
  AddDhanClient<FifteenTF>(services);
}

static void AddDhanClient<T>(IServiceCollection services) where T : Candle
{
  services.AddHttpClient<DhanClient<T>>((sp, client) =>
  {
    var config = sp.GetRequiredService<IOptions<DhanConfig>>().Value;
    client.BaseAddress = new Uri(config.BaseUrl);
    foreach (var h in config.Headers)
      client.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value);
  })
  .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
  {
    AutomaticDecompression =
      System.Net.DecompressionMethods.GZip |
      System.Net.DecompressionMethods.Deflate |
      System.Net.DecompressionMethods.Brotli
  });
}

static void AddSingletons(IServiceCollection services, RunConfig runConfig)
{
  services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
  services.AddSingleton<IMarketApiFactory<DailyTF>, MarketApiFactory<DailyTF>>();
  services.AddSingleton<IMarketApiFactory<FifteenTF>, MarketApiFactory<FifteenTF>>();
  services.AddSingleton(runConfig);
}

static void AddScopes(IServiceCollection services)
{
  services.AddScoped<DataFetchService<DailyTF>>();
  services.AddScoped<DataFetchService<FifteenTF>>();
  services.AddScoped<DataProcessingService>();
  services.AddScoped<AnalysisService>();
  services.AddScoped<DatabaseCleanUpService>();
  services.AddScoped<TableInitializationService>();
}

public class DhanConfig
{
  public string BaseUrl { get; set; } = "https://tv-web.dhan.co";
  public Dictionary<string, string> Headers { get; set; } = new();
}
