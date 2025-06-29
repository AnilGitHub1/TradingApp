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

        public async Task<DailyTFData> GetDailyTFDataByIdAsync(int id)
        {
            return await _context.DailyTFData.FindAsync(id);
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
            var dailyTFData = await GetDailyTFDataByIdAsync(id);
            if (dailyTFData != null)
            {
                _context.DailyTFData.Remove(dailyTFData);
                await _context.SaveChangesAsync();
            }
        }
    }
}