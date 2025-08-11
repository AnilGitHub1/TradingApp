using Microsoft.AspNetCore.Mvc;
using TradingApp.Infrastructure.Data;
using TradingApp.Core.Interfaces;
using TradingApp.Core.Entities;

namespace TradingApp.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class HighLowController : ControllerBase
    {
        private readonly TradingDbContext _context;
        private readonly IHighLowRepository _highLowRepository;

        public HighLowController(TradingDbContext context, IHighLowRepository highlowRepository)
        {
            _context = context;
            _highLowRepository = highlowRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IList<HighLow>>> GetHighLow([FromQuery] int token, [FromQuery] int page = 1, [FromQuery] int pageSize = 1)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Page and pageSize must be greater than 0.");

            var data = await _highLowRepository.GetHighLowAsync(token, page, pageSize);

            if (data == null)
                return NotFound("No HighLow time frame  found.");

            var response = data.Select(d => new
            {
                d.id,
                d.token,
                d.time,
                d.open,
                d.high,
                d.low,
                d.close,
                d.volume,
                d.hl,
                d.tf
            }).ToList();

            return Ok(response);
        }

        [HttpGet("byToken")]
        public async Task<ActionResult<IList<HighLow>>> GetHighLowByToken([FromQuery] int token, int limit)
        {
            IList<HighLow> data;
            if (limit > 0)
                data = await _highLowRepository.GetHighLowAsync(token, limit);
            else
                data = await _highLowRepository.GetAllHighLowAsync(token);

            if (data == null)
                return NotFound("No HighLow time frame  found.");

            var response = data.Select(d => new
            {
                d.id,
                d.token,
                d.time,
                d.open,
                d.high,
                d.low,
                d.close,
                d.volume,
                d.hl,
                d.tf
            }).ToList();

            return Ok(response);
        }
    }
}