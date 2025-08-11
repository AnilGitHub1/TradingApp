using TradingApp.Core.DTOs;

namespace TradingApp.Core.Interfaces
{
    public interface IDailyTFService
    {
        Task<IEnumerable<DailyTFDto>> GetAllAsync();
        Task<DailyTFDto?> GetByIdAsync(int id);
        Task AddAsync(DailyTFDto dto);
        Task UpdateAsync(DailyTFDto dto);
        Task DeleteAsync(int id);
    }
}
