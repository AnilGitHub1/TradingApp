using Microsoft.EntityFrameworkCore;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure.Data;
using YourProject.Repositories;

namespace TradingApp.Infrastructure.Repositories
{
    public class UserTrendlineRepository : Repository<UserTrendline>, IUserTrendlineRepository
    {
        public UserTrendlineRepository(TradingDbContext context) : base(context)
        {
        }

        public async Task<IList<UserTrendline>> GetByUserAsync(int userId)
        {
            return await Context.UserTrendlines
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();
        }

        public async Task<UserTrendline?> GetByIdForUserAsync(int id, int userId)
        {
            return await Context.UserTrendlines
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        }

        public async Task<IList<UserTrendline>> GetByUserAndTokenAsync(int userId, int token, string? tf = null)
        {
            var query = Context.UserTrendlines
                .Where(x => x.UserId == userId && x.Token == token);

            if (!string.IsNullOrWhiteSpace(tf))
            {
                query = query.Where(x => x.Tf == tf);
            }

            return await query
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();
        }

        public async Task AddUserTrendlineAsync(UserTrendline trendline)
        {
            await Context.UserTrendlines.AddAsync(trendline);
            await Context.SaveChangesAsync();
        }

        public async Task UpdateUserTrendlineAsync(UserTrendline trendline)
        {
            Context.UserTrendlines.Update(trendline);
            await Context.SaveChangesAsync();
        }

        public async Task DeleteUserTrendlineAsync(UserTrendline trendline)
        {
            Context.UserTrendlines.Remove(trendline);
            await Context.SaveChangesAsync();
        }
    }
}
