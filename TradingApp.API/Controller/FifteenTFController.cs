using Microsoft.AspNetCore.Mvc;
using TradingApp.API.Models;
using TradingApp.Infrastructure.Data;
using TradingApp.Core.Interfaces;
using TradingApp.Core.Entities;
using TradingApp.Shared.Services;
using TradingApp.Shared.Constants;

namespace TradingApp.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class FifteenTFController : ControllerBase
    {
        private readonly TradingDbContext _context;
        private readonly IFifteenTFRepository _fifteenTFRepository;

        public FifteenTFController(TradingDbContext context, IFifteenTFRepository FifteenTFRepository)
        {
            _context = context;
            _fifteenTFRepository = FifteenTFRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Candle>>> GetFifteenTF([FromQuery] int token, [FromQuery] int page = 1, [FromQuery] int pageSize = 1)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Page and pageSize must be greater than 0.");

            var data = await _fifteenTFRepository.GetFifteenTFAsync(token, page, pageSize);

            if (data == null)
                return NotFound("No Fifteen time frame  found.");

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

        [HttpGet("stockdata")]
        public async Task<ActionResult<IEnumerable<Candle>>> GetFifteenTFByToken([FromQuery] int token, int limit, string tf)
        {
            IEnumerable<Candle> candleData;
            candleData = await _fifteenTFRepository.GetAllFifteenTFAsync(token);
            candleData = Utility.Resample(EnumMapper.GetTimeFrame(tf), candleData, limit);

            if (candleData == null)
                return NotFound("No data found.");

            var stockData = candleData.Select(d => new
            {
                time = new DateTimeOffset(d.time.ToUniversalTime()).ToUnixTimeSeconds(),
                d.open,
                d.high,
                d.low,
                d.close,
                d.volume
            }).ToList();
            var response = new {stockData
              };
            return Ok(response);
        }
    }
}