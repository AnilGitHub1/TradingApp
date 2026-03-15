using Microsoft.EntityFrameworkCore;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure.Data;
<<<<<<< HEAD
using EFCore.BulkExtensions;
=======
>>>>>>> abcae4471c012cc6817891571c67a4d26bae5c70
using YourProject.Repositories;

namespace TradingApp.Infrastructure.Repositories
{
<<<<<<< HEAD
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
=======
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(TradingDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await Context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token);
        }

        public async Task<RefreshToken?> GetActiveByUserIdAsync(int userId)
        {
            var now = DateTime.UtcNow;
            return await Context.RefreshTokens
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Revoked == null && x.Expires > now);
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            await Context.RefreshTokens.AddAsync(refreshToken);
            await Context.SaveChangesAsync();
        }

        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            Context.RefreshTokens.Update(refreshToken);
            await Context.SaveChangesAsync();
        }
    }
>>>>>>> abcae4471c012cc6817891571c67a4d26bae5c70
}
