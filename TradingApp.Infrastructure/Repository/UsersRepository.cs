using Microsoft.EntityFrameworkCore;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure.Data;
using EFCore.BulkExtensions;
using YourProject.Repositories;

namespace TradingApp.Infrastructure.Repositories
{
  public class UsersRepository : Repository<Users>, IUsersRepository
  {

    public UsersRepository(TradingDbContext context) : base(context)
    {
    }

    public async Task<Users> GetUserAsync(string name, string email)
    {
        var user = await Context.Users
            .Where(d => d.name == name && d.email == email)
            .FirstOrDefaultAsync();
        user ??= Users.EmptyUser();
        return user;
    }
    
    public async Task AddUserAsync(Users user)
    {
      await Context.Users.AddAsync(user);
      await Context.SaveChangesAsync();
    }
    
    public async Task UpdateUserAsync(Users user)
    {
      Context.Users.Update(user);
      await Context.SaveChangesAsync();
    }      

    public async Task DeleteUserAsync(int id)
    {
      await Context.Users
      .Where(d => d.id == id)
      .ExecuteDeleteAsync();
    }

    public async Task DeleteUserAsync(Users user)
    {
      if (user != null)
        {
            await Context.Users
            .Where(d => d.id == user.id)
            .ExecuteDeleteAsync();
        }
    }
  }
}
