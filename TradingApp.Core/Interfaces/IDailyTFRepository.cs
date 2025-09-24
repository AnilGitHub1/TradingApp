using TradingApp.Core.DTOs;
using TradingApp.Core.Entities;
using YourProject.Interfaces;

namespace TradingApp.Core.Interfaces
{
  public interface IDailyTFRepository : IRepository<Candle>
  {
    Task<IList<Candle>> GetDailyTFAsync(int token, int page, int pageSize);
    Task<IList<Candle>> GetDailyTFAsync(int token, int limit);
    Task<IList<Candle>> GetAllDailyTFAsync(int token);
    Task<IList<Candle>> GetAllDailyTFAsync(int token, DateTime from);
    Task<IList<Candle>> GetAllDailyTFAsync(int token, DateTime from, DateTime to);
    Task AddDailyTFAsync(Candle Candle);
    Task AddDailyTFAsync(IList<Candle> Candle);
    Task UpdateDailyTFAsync(Candle Candle);
    Task DeleteDailyTFAsync(int id);
    Task DeleteDailyTFAsync(Candle entity);
    Task<int> DeleteDailyTFDataAsync(int token);
    Task<DateTime> GetLatestDateTimeAsync(int token);
  }
}