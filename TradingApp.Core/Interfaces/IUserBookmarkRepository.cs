using TradingApp.Core.Entities;
using YourProject.Interfaces;

namespace TradingApp.Core.Interfaces
{
    public interface IUserBookmarkRepository : IRepository<UserBookmark>
    {
        Task<IList<UserBookmark>> GetByUserAsync(int userId);
        Task<UserBookmark?> GetByIdForUserAsync(int id, int userId);
        Task<UserBookmark?> GetByUserAndTokenAsync(int userId, int token);
        Task AddUserBookmarkAsync(UserBookmark bookmark);
        Task UpdateUserBookmarkAsync(UserBookmark bookmark);
        Task DeleteUserBookmarkAsync(UserBookmark bookmark);
    }
}
