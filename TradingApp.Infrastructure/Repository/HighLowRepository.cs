using Microsoft.EntityFrameworkCore;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure.Data;
using EFCore.BulkExtensions;
using YourProject.Repositories;

namespace TradingApp.Infrastructure.Repositories
{
  public class HighLowRepository : Repository<HighLow>, IHighLowRepository
  {

    public HighLowRepository(TradingDbContext context) : base(context)
    {
    }

    public async Task<IList<HighLow>> GetHighLowAsync(int token, int page, int pageSize)
    {
        if (page <= 0 || pageSize <= 0)
            throw new ArgumentException("Page and pageSize must be greater than 0.");

        return await Context.HighLow
            .Where(d => d.token == token)
            .OrderByDescending(d => d.time)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IList<HighLow>> GetHighLowAsync(int token, int limit)
    {
        return await Context.HighLow
            .Where(d => d.token == token)
            .OrderByDescending(d => d.time)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IList<HighLow>> GetAllHighLowAsync(int token)
    {
        return await Context.HighLow
            .AsNoTracking()
            .Where(x => x.token == token)
            .OrderBy(x => x.time)
            .ToListAsync();
    }

    public async Task<IList<HighLow>> GetAllHighLowAsync(int token, DateTime from)
    {
        return await Context.HighLow
            .AsNoTracking()
            .Where(x => x.token == token && x.time >= from)
            .OrderBy(x => x.time)
            .ToListAsync();
    }

    public async Task<IList<HighLow>> GetAllHighLowAsync(int token, DateTime from, DateTime to)
    {
        return await Context.HighLow
            .AsNoTracking()
            .Where(x => x.token == token && x.time >= from && x.time <= to)
            .OrderBy(x => x.time)
            .ToListAsync();
    }

    public async Task<HighLow?> GetLatestHighLowAsync(int token)
    {
      return await Context.HighLow
          .AsNoTracking()
          .Where(x => x.token == token)
          .OrderByDescending(x => x.time)
          .FirstOrDefaultAsync();
    }
    public async Task<HighLow?> GetLatestHighLowAsync(int token, string tf)
    {
        return await Context.HighLow
            .AsNoTracking()
            .Where(x => x.token == token && x.tf.Contains(tf))
            .OrderByDescending(x => x.time)
            .FirstOrDefaultAsync();
    }
    public async Task AddHighLowAsync(HighLow HighLow)
{
  await Context.HighLow.AddAsync(HighLow);
  await Context.SaveChangesAsync();
}

    public async Task AddHighLowAsync(IList<HighLow> entities)
    {
      if (entities.Count() <= SmallMediumBatchThreshold)
      {
          await Context.HighLow.AddRangeAsync(entities);
          await Context.SaveChangesAsync();
      }
      else
        {
            // Switch to bulk insert (library or raw SQL)
            await Context.BulkInsertAsync(entities);
        }
    }

    public async Task UpdateHighLowAsync(HighLow HighLow)
    {
      Context.HighLow.Update(HighLow);
      await Context.SaveChangesAsync();
    }      

    public async Task DeleteHighLowAsync(int id)
    {
      await Context.HighLow
      .Where(d => d.id == id)
      .ExecuteDeleteAsync();
    }

    public async Task DeleteHighLowAsync(HighLow entity)
    {
      if (entity != null)
        {
            await Context.HighLow
            .Where(d => d.id == entity.id)
            .ExecuteDeleteAsync();
        }
    }
    
    public async Task<int> DeleteHighLowDataAsync(int token)
    {
      // Raw SQL is MUCH faster than EF RemoveRange for large sets nad bulk operations
      return await Context.Database.ExecuteSqlRawAsync(
            "DELETE FROM HighLow_data WHERE token = {0} ", token
        );
    }
    
    public async Task<HighLow?> GetLatestCandleAsync(int token)
    {
      return await Context.HighLow
        .AsNoTracking()
        .Where(x => x.token == token)
        .OrderByDescending(x => x.time)
        .FirstOrDefaultAsync();
    }        
  }
}
