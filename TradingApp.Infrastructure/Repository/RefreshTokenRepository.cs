using Microsoft.EntityFrameworkCore;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure.Data;
using YourProject.Repositories;

namespace TradingApp.Infrastructure.Repositories
{
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
}
