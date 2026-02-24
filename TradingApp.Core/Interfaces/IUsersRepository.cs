using System.Reflection.Metadata.Ecma335;
using TradingApp.Core.Entities;
using YourProject.Interfaces;

namespace TradingApp.Core.Interfaces
{
  public interface IUsersRepository : IRepository<Users>
  {
    Task<Users> GetUserAsync(string name, string email);
    Task AddUserAsync(Users user);
    Task UpdateUserAsync(Users user);
    Task DeleteUserAsync(int id);
    Task DeleteUserAsync(Users user);
  }
}