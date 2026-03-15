using TradingApp.Core.Entities;
using YourProject.Interfaces;

namespace TradingApp.Core.Interfaces
{
  public interface IRefreshTokenRepository : IRepository<RefreshToken>
  {
    Task<RefreshToken> GetRefreshTokenAsync(int userId);
    Task AddRefreshTokenAsync(RefreshToken refreshToken);
    Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
    Task DeleteRefreshTokenAsync(int id);
    Task DeleteRefreshTokenAsync(RefreshToken refreshToken);
  }
}