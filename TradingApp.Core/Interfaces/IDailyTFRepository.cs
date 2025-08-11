using TradingApp.Core.Entities;
using YourProject.Interfaces;

namespace TradingApp.Core.Interfaces
{
    public interface IDailyTFRepository : IRepository<DailyTF>
    {
        Task<IList<DailyTF>> GetDailyTFAsync(int token, int page, int pageSize);
        Task<IList<DailyTF>> GetDailyTFAsync(int token, int limit);
        Task<IList<DailyTF>> GetAllDailyTFAsync(int token);
        Task<IList<DailyTF>> GetAllDailyTFAsync(int token, DateTime from);
        Task<IList<DailyTF>> GetAllDailyTFAsync(int token, DateTime from, DateTime to);
        Task AddDailyTFAsync(DailyTF DailyTF);
        Task AddDailyTFAsync(IList<DailyTF> DailyTF);
        Task UpdateDailyTFAsync(DailyTF DailyTF);
        Task DeleteDailyTFAsync(int id);
        Task DeleteDailyTFAsync(DailyTF entity);
        Task<int> DeleteDailyTFDataAsync(int token);
    }
}