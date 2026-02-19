using Microsoft.AspNetCore.Mvc;
using TradingApp.Infrastructure.Data;
using TradingApp.Core.Interfaces;
using TradingApp.Core.Entities;

namespace TradingApp.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly TradingDbContext _context;
        private readonly ITrendlineRepository _TrendlineRepository;

        public UsersController(TradingDbContext context, ITrendlineRepository TrendlineRepository)
        {
            _context = context;
            _TrendlineRepository = TrendlineRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IList<Users>>> GetUsers()
        {
          return Ok();
        }      
      }
}