using TradingApp.Core.Entities;
using YourProject.Interfaces;

namespace TradingApp.Core.Interfaces
{
    public interface IHighLowRepository : IRepository<HighLow>
    {
        Task<IList<HighLow>> GetHighLowAsync(int token, int page, int pageSize);
        Task<IList<HighLow>> GetHighLowAsync(int token, int limit);
        Task<IList<HighLow>> GetAllHighLowAsync(int token);
        Task<IList<HighLow>> GetAllHighLowAsync(int token, DateTime from);
        Task<IList<HighLow>> GetAllHighLowAsync(int token, DateTime from, DateTime to);
        Task AddHighLowAsync(HighLow HighLow);
        Task AddHighLowAsync(IList<HighLow> HighLow);
        Task UpdateHighLowAsync(HighLow HighLow);
        Task DeleteHighLowAsync(int id);
        Task DeleteHighLowAsync(HighLow entity);
        Task<int> DeleteHighLowDataAsync(int token);
    }
}