using TradingApp.Core.Entities;
using YourProject.Interfaces;

namespace TradingApp.Core.Interfaces
{
    public interface IUserTrendlineRepository : IRepository<UserTrendline>
    {
        Task<IList<UserTrendline>> GetByUserAsync(int userId);
        Task<UserTrendline?> GetByIdForUserAsync(int id, int userId);
        Task<IList<UserTrendline>> GetByUserAndTokenAsync(int userId, int token, string? tf = null);
        Task AddUserTrendlineAsync(UserTrendline trendline);
        Task UpdateUserTrendlineAsync(UserTrendline trendline);
        Task DeleteUserTrendlineAsync(UserTrendline trendline);
    }
}
