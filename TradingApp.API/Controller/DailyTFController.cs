using Microsoft.AspNetCore.Mvc;
using TradingApp.Infrastructure.Data;
using TradingApp.Core.Interfaces;
using TradingApp.Core.Entities;

namespace TradingApp.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class DailyTFController : ControllerBase
    {
        private readonly TradingDbContext _context;
        private readonly IDailyTFRepository _dailyTFRepository;

        public DailyTFController(TradingDbContext context, IDailyTFRepository dailyTFRepository)
        {
            _context = context;
            _dailyTFRepository = dailyTFRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Candle>>> GetDailyTF([FromQuery] int token, [FromQuery] int page = 1, [FromQuery] int pageSize = 1)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Page and pageSize must be greater than 0.");

            var data = await _dailyTFRepository.GetDailyTFAsync(token, page, pageSize);

            if (data == null)
                return NotFound("No daily time frame  found.");

            var response = data.Select(d => new
            {
                d.id,
                d.token,
                d.time,
                d.open,
                d.high,
                d.low,
                d.close,
                d.volume
            }).ToList();

            return Ok(response);
        }

        [HttpGet("byToken")]
        public async Task<ActionResult<IEnumerable<Candle>>> GetDailyTFByToken([FromQuery] int token, int limit)
        {
            IEnumerable<Candle> data;
            if (limit > 0)
                data = await _dailyTFRepository.GetDailyTFAsync(token, limit);
            else
                data = await _dailyTFRepository.GetAllDailyTFAsync(token);

            if (data == null)
                return NotFound("No daily time frame  found.");

            var response = data.Select(d => new
            {
                d.id,
                d.token,
                d.time,
                d.open,
                d.high,
                d.low,
                d.close,
                d.volume
            }).ToList();

            return Ok(response);
        }
    }
}