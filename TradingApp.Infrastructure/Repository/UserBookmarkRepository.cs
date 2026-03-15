using Microsoft.EntityFrameworkCore;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure.Data;
using YourProject.Repositories;

namespace TradingApp.Infrastructure.Repositories
{
    public class UserBookmarkRepository : Repository<UserBookmark>, IUserBookmarkRepository
    {
        public UserBookmarkRepository(TradingDbContext context) : base(context)
        {
        }

        public async Task<IList<UserBookmark>> GetByUserAsync(int userId)
        {
            return await Context.UserBookmarks
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<UserBookmark?> GetByIdForUserAsync(int id, int userId)
        {
            return await Context.UserBookmarks
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        }

        public async Task<UserBookmark?> GetByUserAndTokenAsync(int userId, int token)
        {
            return await Context.UserBookmarks
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Token == token);
        }

        public async Task AddUserBookmarkAsync(UserBookmark bookmark)
        {
            await Context.UserBookmarks.AddAsync(bookmark);
            await Context.SaveChangesAsync();
        }

        public async Task UpdateUserBookmarkAsync(UserBookmark bookmark)
        {
            Context.UserBookmarks.Update(bookmark);
            await Context.SaveChangesAsync();
        }

        public async Task DeleteUserBookmarkAsync(UserBookmark bookmark)
        {
            Context.UserBookmarks.Remove(bookmark);
            await Context.SaveChangesAsync();
        }
    }
}
