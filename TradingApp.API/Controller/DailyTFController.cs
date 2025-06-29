using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradingApp.API.Data;
using TradingApp.API.Models;
using TradingApp.API.Repository.Interface;

namespace TradingApp.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class DailyTFController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IDailyTFRepository _dailyTFRepository;

        public DailyTFController(MyDbContext context, IDailyTFRepository dailyTFRepository)
        {
            _context = context;
            _dailyTFRepository = dailyTFRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IList<DailyTFData>>> GetDailyTFData([FromQuery] int page = 1, [FromQuery] int pageSize = 1)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Page and pageSize must be greater than 0.");

            var data = await _dailyTFRepository.GetAllDailyTFDataAsync(page, pageSize);

            if (data == null)
                return NotFound("No daily time frame data found.");

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