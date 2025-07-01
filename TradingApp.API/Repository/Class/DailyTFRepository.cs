using Microsoft.EntityFrameworkCore;
using TradingApp.API.Data;
using TradingApp.API.Models;
using TradingApp.API.Repository.Interface;

namespace TradingApp.API.Repository.Class
{
    public class DailyTFRepository : IDailyTFRepository
    {
        private readonly MyDbContext _context;

        public DailyTFRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IList<DailyTFData>> GetAllDailyTFDataAsync(int page, int pageSize)
        {
            if (page <= 0 || pageSize <= 0)
                throw new ArgumentException("Page and pageSize must be greater than 0.");

            return await _context.DailyTFData
                .OrderByDescending(d => d.time) 
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IList<DailyTFData>> GetDailyTFDataByTokenAsync(int token, int limit)
        {
            return await _context.DailyTFData
                .Where(d => d.token == token)
                .OrderByDescending(d => d.time) 
                .Take(limit)
                .ToListAsync();
        }

        public async Task AddDailyTFDataAsync(DailyTFData dailyTFData)
        {
            await _context.DailyTFData.AddAsync(dailyTFData);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDailyTFDataAsync(DailyTFData dailyTFData)
        {
            _context.DailyTFData.Update(dailyTFData);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDailyTFDataAsync(int id)
        {
            await _context.DailyTFData
                .Where(d => d.id == id)
                .ExecuteDeleteAsync();
        }
    }
}