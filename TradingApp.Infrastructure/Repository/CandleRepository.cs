using Microsoft.EntityFrameworkCore;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;
using TradingApp.Infrastructure.Data;
using EFCore.BulkExtensions;
using YourProject.Repositories;
using TradingApp.Core.DTOs;

namespace TradingApp.Infrastructure.Repositories
{
  public class CandleRepository<T> : Repository<T>, ICandleRepository<T> where T : Candle
  {
    protected readonly TradingDbContext _context;
    protected readonly DbSet<T> _dbSet;
    public CandleRepository(TradingDbContext context) : base(context)
    {
      _context = context;
      _dbSet = context.Set<T>();
    }

    public async Task<IList<T>> GetAsync(int token, int page, int pageSize)
    {
      if (page <= 0 || pageSize <= 0)
        throw new ArgumentException("Page and pageSize must be greater than 0.");

      return await _dbSet
          .Where(d => d.token == token)
          .OrderByDescending(d => d.time)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .ToListAsync();
    }

    public async Task<IList<T>> GetAsync(int token, int limit)
    {
      return await _dbSet
          .Where(d => d.token == token)
          .OrderByDescending(d => d.time)
          .Take(limit)
          .ToListAsync();
    }

    public async Task<IList<T>> GetAllAsync(int token)
    {
      return await _dbSet
          .AsNoTracking()
          .Where(x => x.token == token)
          .OrderBy(x => x.time)
          .ToListAsync();
    }

    public async Task<IList<T>> GetAllAsync(int token, DateTime from)
    {
      return await _dbSet
          .AsNoTracking()
          .Where(x => x.token == token && x.time >= from)
          .OrderBy(x => x.time)
          .ToListAsync();
    }

    public async Task<IList<T>> GetAllAsync(int token, DateTime from, DateTime to)
    {
      return await _dbSet
          .AsNoTracking()
          .Where(x => x.token == token && x.time >= from && x.time <= to)
          .OrderBy(x => x.time)
          .ToListAsync();
    }

    public async Task AddAsync(IList<T> candles)
    {
      if (candles == null || !candles.Any()) return;

      if (candles.Count() <= SmallMediumBatchThreshold)
      {
        await _dbSet.AddRangeAsync(candles);
        await Context.SaveChangesAsync();
      }
      else
      {
        // Switch to bulk insert (library or raw SQL)
        await Context.BulkInsertAsync(candles);
      }
    }

    public async Task UpdateAsync(T entity)
    {
      _dbSet.Update(entity);
      await Context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
      await _dbSet
      .Where(d => d.id == id)
      .ExecuteDeleteAsync();
    }

    public async Task DeleteAsync(T entity)
    {
      if (entity != null)
      {
        await _dbSet
        .Where(d => d.id == entity.id)
        .ExecuteDeleteAsync();
      }
    }

    public async Task<int> DeleteDataAsync(int token)
    {
      // Raw SQL is MUCH faster than EF RemoveRange for large sets nad bulk operations
      return await Context.Database.ExecuteSqlRawAsync(
          "DELETE FROM _data WHERE token = {0} ", token
      );
    }

    public async Task<T?> GetLatestCandleAsync(int token)
    {
      return await _dbSet
        .AsNoTracking()
        .Where(x => x.token == token)
        .OrderByDescending(x => x.time)
        .FirstOrDefaultAsync();
    }
    public async Task<DateTime> GetLatestDateTimeAsync(int token)
    {
      var latest = await _dbSet
        .Where(x => x.token == token)
        .OrderByDescending(x => x.time)
        .Select(x => x.time)
        .FirstOrDefaultAsync();

      return latest == default ? DateTime.MinValue : latest;
    }
  }
}