using Microsoft.EntityFrameworkCore;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure.Data;
using EFCore.BulkExtensions;
using YourProject.Repositories;
using TradingApp.Core.DTOs;

namespace TradingApp.Infrastructure.Repositories
{
  public class FifteenTFRepository : Repository<Candle>, IFifteenTFRepository
  {

    public FifteenTFRepository(TradingDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Candle>> GetFifteenTFAsync(int token, int page, int pageSize)
    {
      if (page <= 0 || pageSize <= 0)
        throw new ArgumentException("Page and pageSize must be greater than 0.");

      return await Context.FifteenTF
          .Where(d => d.token == token)
          .OrderByDescending(d => d.time)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .ToListAsync();
    }

    public async Task<IEnumerable<Candle>> GetFifteenTFAsync(int token, int limit)
    {
      return await Context.FifteenTF
          .Where(d => d.token == token)
          .OrderByDescending(d => d.time)
          .Take(limit)
          .ToListAsync();
    }

    public async Task<IEnumerable<Candle>> GetAllFifteenTFAsync(int token)
    {
      return await Context.FifteenTF
          .AsNoTracking()
          .Where(x => x.token == token)
          .OrderBy(x => x.time)
          .ToListAsync();
    }

    public async Task<IEnumerable<Candle>> GetAllFifteenTFAsync(int token, DateTime from)
    {
      return await Context.FifteenTF
          .AsNoTracking()
          .Where(x => x.token == token && x.time >= from)
          .OrderBy(x => x.time)
          .ToListAsync();
    }

    public async Task<IEnumerable<Candle>> GetAllFifteenTFAsync(int token, DateTime from, DateTime to)
    {
      return await Context.FifteenTF
          .AsNoTracking()
          .Where(x => x.token == token && x.time >= from && x.time <= to)
          .OrderBy(x => x.time)
          .ToListAsync();
    }

    public async Task AddFifteenTFAsync(FifteenTF Candle)
    {
      await Context.FifteenTF.AddAsync(Candle);
      await Context.SaveChangesAsync();
    }

    public async Task AddFifteenTFAsync(IEnumerable<FifteenTF> entities)
    {
      if (entities == null || !entities.Any()) return;

      if (entities.Count() <= SmallMediumBatchThreshold)
      {
        await Context.FifteenTF.AddRangeAsync(entities);
        await Context.SaveChangesAsync();
      }
      else
      {
        // Switch to bulk insert (library or raw SQL)
        await Context.BulkInsertAsync(entities);
      }
    }
    public async Task UpdateFifteenTFAsync(FifteenTF Candle)
    {
      Context.FifteenTF.Update(Candle);
      await Context.SaveChangesAsync();
    }

    public async Task DeleteFifteenTFAsync(int id)
    {
      await Context.FifteenTF
      .Where(d => d.id == id)
      .ExecuteDeleteAsync();
    }

    public async Task DeleteFifteenTFAsync(FifteenTF entity)
    {
      if (entity != null)
      {
        await Context.FifteenTF
        .Where(d => d.id == entity.id)
        .ExecuteDeleteAsync();
      }
    }

    public async Task<int> DeleteFifteenTFDataAsync(int token)
    {
      // Raw SQL is MUCH faster than EF RemoveRange for large sets nad bulk operations
      return await Context.Database.ExecuteSqlRawAsync(
          "DELETE FROM Fifteentf_data WHERE token = {0} ", token
      );
    }

    public async Task<Candle?> GetLatestCandleAsync(int token)
    {
      return await Context.FifteenTF
          .AsNoTracking()
          .Where(x => x.token == token)
          .OrderByDescending(x => x.time)
          .FirstOrDefaultAsync();
    }
    public async Task<DateTime> GetLatestDateTimeAsync(int token)
    {
      var latest = await Context.FifteenTF
        .Where(x => x.token == token)
        .OrderByDescending(x => x.time)
        .Select(x => x.time)
        .FirstOrDefaultAsync();

      return latest == default ? DateTime.MinValue : latest;
    }
  }
}
