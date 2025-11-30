using TradingApp.Core.DTOs;
using TradingApp.Core.Entities;
using YourProject.Interfaces;

namespace TradingApp.Core.Interfaces
{
  public interface IDailyTFRepository : IRepository<Candle>
  {
    Task<IEnumerable<Candle>> GetDailyTFAsync(int token, int page, int pageSize);
    Task<IEnumerable<Candle>> GetDailyTFAsync(int token, int limit);
    Task<IEnumerable<Candle>> GetAllDailyTFAsync(int token);
    Task<IEnumerable<Candle>> GetAllDailyTFAsync(int token, DateTime from);
    Task<IEnumerable<Candle>> GetAllDailyTFAsync(int token, DateTime from, DateTime to);
    Task AddDailyTFAsync(DailyTF Candle);
    Task AddDailyTFAsync(IEnumerable<DailyTF> Candle);
    Task UpdateDailyTFAsync(DailyTF Candle);
    Task DeleteDailyTFAsync(int id);
    Task DeleteDailyTFAsync(DailyTF entity);
    Task<int> DeleteDailyTFDataAsync(int token);
    Task<DateTime> GetLatestDateTimeAsync(int token);
  }
}