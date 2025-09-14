using TradingApp.Core.Entities;

namespace TradingApp.Core.Interfaces
{
  public interface ICandleRepository<T> where T : Candle
  {
      Task<IList<T>> GetAsync(int token, int page, int pageSize);
      Task<IList<T>> GetAsync(int token, int limit);
      Task<IList<T>> GetAllAsync(int token);
      Task<IList<T>> GetAllAsync(int token, DateTime from);
      Task<IList<T>> GetAllAsync(int token, DateTime from, DateTime to);
      Task AddAsync(T entity);
      Task AddAsync(IList<T> entities);
      Task UpdateAsync(T entity);
      Task DeleteAsync(int id);
      Task DeleteAsync(T entity);
      Task<int> DeleteDataAsync(int token);
      Task<DateTime> GetLatestDateTimeAsync(int token);
  }
}