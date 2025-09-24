using TradingApp.Core.DTOs;
using TradingApp.Core.Entities;
using YourProject.Interfaces;

namespace TradingApp.Core.Interfaces
{
  public interface IFifteenTFRepository : IRepository<Candle>
  {
    Task<IList<Candle>> GetFifteenTFAsync(int token, int page, int pageSize);
    Task<IList<Candle>> GetFifteenTFAsync(int token, int limit);
    Task<IList<Candle>> GetAllFifteenTFAsync(int token);
    Task<IList<Candle>> GetAllFifteenTFAsync(int token, DateTime from);
    Task<IList<Candle>> GetAllFifteenTFAsync(int token, DateTime from, DateTime to);
    Task AddFifteenTFAsync(Candle Candle);
    Task AddFifteenTFAsync(IList<Candle> Candle);
    Task UpdateFifteenTFAsync(Candle Candle);
    Task DeleteFifteenTFAsync(int id);
    Task DeleteFifteenTFAsync(Candle entity);
    Task<int> DeleteFifteenTFDataAsync(int token);
    Task<DateTime> GetLatestDateTimeAsync(int token);
  }
}