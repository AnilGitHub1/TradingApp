using TradingApp.Core.DTOs;
using TradingApp.Core.Entities;
using TradingApp.Core.Interfaces;

namespace TradingApp.Core.Services
{
    public class DailyTFService : IDailyTFService
    {
        private readonly IDailyTFRepository _repository;

        public DailyTFService(IDailyTFRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DailyTFDto>> GetAllAsync()
        {
            var data = await _repository.GetAllDailyTFAsync(0);
            return data.Select(d => new DailyTFDto
            {
                id = d.id,
                token = d.token,
                time = d.time,
                open = d.open,
                high = d.high,
                low = d.low,
                close = d.close,
                volume = d.volume
            });
        }

        public async Task<DailyTFDto?> GetByIdAsync(int id)
        {
            var d = await _repository.GetAllDailyTFAsync(id);
            return null;
        }

        public async Task AddAsync(DailyTFDto dto)
        {
            var entity = new DailyTF
            {
                token = dto.token,
                time = dto.time,
                open = dto.open,
                high = dto.high,
                low = dto.low,
                close = dto.close,
                volume = dto.volume
            };
            await _repository.AddDailyTFAsync(entity);
        }

        public async Task UpdateAsync(DailyTFDto dto)
        {
            var entity = new DailyTF
            {
                id = dto.id,
                token = dto.token,
                time = dto.time,
                open = dto.open,
                high = dto.high,
                low = dto.low,
                close = dto.close,
                volume = dto.volume
            };
            await _repository.UpdateDailyTFAsync(entity);
        }

        public async Task DeleteAsync(int id) => await _repository.DeleteDailyTFAsync(id);
    }
}
