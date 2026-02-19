using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TradingApp.Infrastructure.Data;
using TradingApp.Infrastructure.Repositories;
using TradingApp.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// -------------------- SERVICES --------------------

builder.Services.AddDbContext<TradingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IDailyTFRepository, DailyTFRepository>();
builder.Services.AddScoped<IFifteenTFRepository, FifteenTFRepository>();
builder.Services.AddScoped<IHighLowRepository, HighLowRepository>();
builder.Services.AddScoped<ITrendlineRepository, TrendlineRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Trading API", Version = "v1" });
});

var app = builder.Build();

// -------------------- MIDDLEWARE (ORDER MATTERS) --------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Trading API v1");
    });
}

// SERVE wwwroot (your CHART.html)
app.UseStaticFiles();

app.UseHttpsRedirection();

// ENABLE CORS (MUST be before MapControllers)
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();