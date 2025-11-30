using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TradingApp.Infrastructure.Data;
using TradingApp.Infrastructure.Repositories;
using TradingApp.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TradingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Trading API", Version = "v1" });
});

builder.Services.AddScoped<IDailyTFRepository, DailyTFRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Trading API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
