using TradingApp.Core.DTOs;
using TradingApp.Core.Entities;
using YourProject.Interfaces;

namespace TradingApp.Core.Interfaces
{
  public interface IFifteenTFRepository : IRepository<Candle>
  {
    Task<IEnumerable<Candle>> GetFifteenTFAsync(int token, int page, int pageSize);
    Task<IEnumerable<Candle>> GetFifteenTFAsync(int token, int limit);
    Task<IEnumerable<Candle>> GetAllFifteenTFAsync(int token);
    Task<IEnumerable<Candle>> GetAllFifteenTFAsync(int token, DateTime from);
    Task<IEnumerable<Candle>> GetAllFifteenTFAsync(int token, DateTime from, DateTime to);
    Task AddFifteenTFAsync(FifteenTF Candle);
    Task AddFifteenTFAsync(IEnumerable<FifteenTF> Candles);
    Task UpdateFifteenTFAsync(FifteenTF Candle);
    Task DeleteFifteenTFAsync(int id);
    Task DeleteFifteenTFAsync(FifteenTF entity);
    Task<int> DeleteFifteenTFDataAsync(int token);
    Task<DateTime> GetLatestDateTimeAsync(int token);
  }
}