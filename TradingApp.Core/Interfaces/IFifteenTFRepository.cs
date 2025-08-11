using TradingApp.Core.Entities;
using YourProject.Interfaces;

namespace TradingApp.Core.Interfaces
{
    public interface IFifteenTFRepository : IRepository<FifteenTF>
    {
        Task<IList<FifteenTF>> GetFifteenTFAsync(int token, int page, int pageSize);
        Task<IList<FifteenTF>> GetFifteenTFAsync(int token, int limit);
        Task<IList<FifteenTF>> GetAllFifteenTFAsync(int token);
        Task<IList<FifteenTF>> GetAllFifteenTFAsync(int token, DateTime from);
        Task<IList<FifteenTF>> GetAllFifteenTFAsync(int token, DateTime from, DateTime to);
        Task AddFifteenTFAsync(FifteenTF FifteenTF);
        Task AddFifteenTFAsync(IList<FifteenTF> FifteenTF);
        Task UpdateFifteenTFAsync(FifteenTF FifteenTF);
        Task DeleteFifteenTFAsync(int id);
        Task DeleteFifteenTFAsync(FifteenTF entity);
        Task<int> DeleteFifteenTFDataAsync(int token);
    }
}