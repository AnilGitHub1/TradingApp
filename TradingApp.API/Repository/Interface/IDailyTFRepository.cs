using TradingApp.API.Models;

namespace TradingApp.API.Repository.Interface
{
    public interface IDailyTFRepository
    {
        Task<IList<DailyTFData>> GetAllDailyTFDataAsync(int page, int pageSize);
        Task<IList<DailyTFData>> GetDailyTFDataByTokenAsync(int token, int limit);
        Task AddDailyTFDataAsync(DailyTFData dailyTFData);
        Task UpdateDailyTFDataAsync(DailyTFData dailyTFData);
        Task DeleteDailyTFDataAsync(int id);
    }
}