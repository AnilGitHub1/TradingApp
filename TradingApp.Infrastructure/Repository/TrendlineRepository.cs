using Microsoft.EntityFrameworkCore;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure.Data;
using EFCore.BulkExtensions;
using YourProject.Repositories;

namespace TradingApp.Infrastructure.Repositories
{
  public class TrendlineRepository : Repository<Trendline>, ITrendlineRepository
  {
    public TrendlineRepository(TradingDbContext context) : base(context)
    {
    }
    public async Task<IList<Trendline>> GetTrendlineAsync(int token, int page, int pageSize)
    {
      if (page <= 0 || pageSize <= 0)
        throw new ArgumentException("Page and pageSize must be greater than 0.");

      return await Context.Trendline
        .Where(d => d.token == token)
        .OrderByDescending(d => d.starttime)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    }
    public async Task<IList<Trendline>> GetTrendlineAsync(int token, int limit)
    {
      return await Context.Trendline
        .Where(d => d.token == token)
        .OrderByDescending(d => d.starttime)
        .Take(limit)
        .ToListAsync();
    }

    public async Task<IList<Trendline>> GetAllTrendlineAsync(int token)
    {
      return await Context.Trendline
        .AsNoTracking()
        .Where(x => x.token == token)
        .OrderBy(x => x.starttime)
        .ToListAsync();
    }

    public async Task<IList<Trendline>> GetAllTrendlineAsync(int token, DateTime from)
  {
      return await Context.Trendline
        .AsNoTracking()
        .Where(x => x.token == token && x.starttime >= from)
        .OrderBy(x => x.starttime)
        .ToListAsync();
    }

    public async Task<IList<Trendline>> GetAllTrendlineAsync(int token, DateTime from, DateTime to)
    {
      return await Context.Trendline
        .AsNoTracking()
        .Where(x => x.token == token && x.starttime >= from && x.starttime <= to)
        .OrderBy(x => x.starttime)
        .ToListAsync();
    }

    public async Task AddTrendlineAsync(Trendline Trendline)
    {
      await Context.Trendline.AddAsync(Trendline);
      await Context.SaveChangesAsync();
    }

    public async Task AddTrendlineAsync(IList<Trendline> entities)
    {
      if (entities.Count() <= SmallMediumBatchThreshold)
      {
        await Context.Trendline.AddRangeAsync(entities);
        await Context.SaveChangesAsync();
      }
      else
      {
        // Switch to bulk insert (library or raw SQL)
        await Context.BulkInsertAsync(entities);
      }
    }

    public async Task UpdateTrendlineAsync(Trendline Trendline)
    {
      Context.Trendline.Update(Trendline);
      await Context.SaveChangesAsync();
    }   

    public async Task UpdateTrendlinesAsync(List<Trendline> newList)
{
    foreach (var incoming in newList)
    {
        // Find the single row that matches this token-tf-hl
        var existing = await Context.Trendline
            .FirstOrDefaultAsync(t =>
                t.token == incoming.token &&
                t.tf == incoming.tf &&
                t.hl == incoming.hl);

        if (existing == null)
        {
            // This should never happen because you pre-created rows.
            // But if missing, create it safely.
            await Context.Trendline.AddAsync(incoming);
            continue;
        }

        // Check if any of the values actually changed
        bool changed =
            existing.starttime != incoming.starttime ||
            existing.endtime != incoming.endtime ||
            existing.slope != incoming.slope ||
            existing.intercept != incoming.intercept ||
            existing.index != incoming.index ||
            existing.index1 != incoming.index1 ||
            existing.index2 != incoming.index2 ||
            existing.connects != incoming.connects ||
            existing.totalconnects != incoming.totalconnects;

        if (changed)
        {
            // Update only the changed properties
            existing.starttime = incoming.starttime;
            existing.endtime = incoming.endtime;
            existing.slope = incoming.slope;
            existing.intercept = incoming.intercept;

            existing.index = incoming.index;
            existing.index1 = incoming.index1;
            existing.index2 = incoming.index2;
            existing.connects = incoming.connects;
            existing.totalconnects = incoming.totalconnects;
        }
    }

    await Context.SaveChangesAsync();
}
    

    public async Task DeleteTrendlineAsync(int id)
    {
      await Context.Trendline
      .Where(d => d.id == id)
      .ExecuteDeleteAsync();
    }

    public async Task DeleteTrendlineAsync(Trendline entity)
    {
      if (entity != null)
      {
        await Context.Trendline
        .Where(d => d.id == entity.id)
        .ExecuteDeleteAsync();
      }
    }
    
    public async Task<int> DeleteTrendlineDataAsync(int token)
    {
      // Raw SQL is MUCH faster than EF RemoveRange for large sets nad bulk operations
      return await Context.Database.ExecuteSqlRawAsync(
        "DELETE FROM Trendline_data WHERE token = {0} ", token
      );
    }
    
    public async Task<Trendline?> GetLatestCandleAsync(int token)
    {
      return await Context.Trendline
        .AsNoTracking()
        .Where(x => x.token == token)
        .OrderByDescending(x => x.starttime)
        .FirstOrDefaultAsync();
    }        
  }
}
