using Microsoft.EntityFrameworkCore;
using TradingApp.Infrastructure.Data;
using YourProject.Interfaces;

namespace YourProject.Repositories
{
  public class Repository<T> : IRepository<T> where T : class
  {
    private readonly TradingDbContext _context;
    private readonly DbSet<T> _dbSet;

    protected TradingDbContext Context => _context;
    protected const int SmallMediumBatchThreshold = 1000;
    public Repository(TradingDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Delete(T entity) => _dbSet.Remove(entity);

    public async Task SaveAsync() => await _context.SaveChangesAsync();
  }
}
