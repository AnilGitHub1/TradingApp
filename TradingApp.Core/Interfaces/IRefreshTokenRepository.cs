using TradingApp.Core.Entities;
using YourProject.Interfaces;

namespace TradingApp.Core.Interfaces
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<RefreshToken?> GetActiveByUserIdAsync(int userId);
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
    }
}
