using Microsoft.AspNetCore.Mvc;
using TradingApp.Infrastructure.Data;
using TradingApp.Core.Interfaces;
using TradingApp.Core.Entities;

namespace TradingApp.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrendlineController : ControllerBase
    {
        private readonly TradingDbContext _context;
        private readonly ITrendlineRepository _TrendlineRepository;

        public TrendlineController(TradingDbContext context, ITrendlineRepository TrendlineRepository)
        {
            _context = context;
            _TrendlineRepository = TrendlineRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IList<Trendline>>> GetTrendline([FromQuery] int token, [FromQuery] int page = 1, [FromQuery] int pageSize = 1)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Page and pageSize must be greater than 0.");

            var data = await _TrendlineRepository.GetTrendlineAsync(token, page, pageSize);

            if (data == null)
                return NotFound("No daily time frame  found.");

            var response = data.Select(d => new
            {
                d.id,
                d.token,
            }).ToList();

            return Ok(response);
        }

        [HttpGet("byToken")]
        public async Task<ActionResult<IList<Trendline>>> GetTrendlineByToken([FromQuery] int token, string tf)
        {
            IList<Trendline> data;
            data = await _TrendlineRepository.GetAllTrendlineAsync(token);

            if (data == null)
                return NotFound("No trendlines found.");

            var response = data.Where(x =>x.tf.Contains(tf) && x.hl == "h" && x.connects > 0).Select(d => new
            {
                d.id,
                d.token,
                d.hl,
                d.tf,
                d.connects,
                d.starttime,
                d.endtime,
                d.index1,
                d.index2,
                d.slope,
                d.intercept
            }).ToList();

            return Ok(response);
        }
    }
}