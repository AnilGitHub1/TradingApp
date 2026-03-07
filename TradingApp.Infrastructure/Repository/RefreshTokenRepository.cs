using Microsoft.EntityFrameworkCore;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure.Data;
using EFCore.BulkExtensions;
using YourProject.Repositories;

namespace TradingApp.Infrastructure.Repositories
{
  public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
  {

    public RefreshTokenRepository(TradingDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken> GetRefreshTokenAsync(int userId)
    {
        var refreshToken = await Context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .FirstOrDefaultAsync();
        refreshToken ??= RefreshToken.EmptyRefreshToken();
        return refreshToken;
    }
    
    public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
    {
      if(!RefreshToken.IsValidRefreshToken(refreshToken)) return;
      await Context.RefreshTokens.AddAsync(refreshToken);
      await Context.SaveChangesAsync();
    }
    
    public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
    {
      Context.RefreshTokens.Update(refreshToken);
      await Context.SaveChangesAsync();
    }      

    public async Task DeleteRefreshTokenAsync(int id)
    {
      await Context.Users
      .Where(d => d.id == id)
      .ExecuteDeleteAsync();
    }

    public async Task DeleteRefreshTokenAsync(RefreshToken refreshToken)
    {
      if (refreshToken != null)
        {
            await Context.RefreshTokens
            .Where(d => d.UserId == refreshToken.Id)
            .ExecuteDeleteAsync();
        }
    }
  }
}
