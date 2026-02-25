using Microsoft.AspNetCore.Mvc;
using TradingApp.Infrastructure.Data;
using TradingApp.Core.Interfaces;
using TradingApp.Core.Entities;
using Microsoft.EntityFrameworkCore;
using TradingApp.Shared.Constants;
using TradingApp.Shared.Services;

namespace TradingApp.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockDetailsController : ControllerBase
    {
        private readonly TradingDbContext _context;
        private readonly ITrendlineRepository _TrendlineRepository;

        public StockDetailsController(TradingDbContext context, ITrendlineRepository TrendlineRepository)
        {
            _context = context;
            _TrendlineRepository = TrendlineRepository;
        }

        [HttpGet("stocklist")]
        public async Task<ActionResult<IList<string>>> GetStockFilter(
            string tf,
            string category,
            string sort)
        {
            tf = EnumMapper.GetTimeFrame(EnumMapper.GetTimeFrame(tf));
            var tokensDict = Utility.GetTokens(EnumMapper.GetStockCategory(category));

            var tokensList = tokensDict.Keys.ToList();

            switch (EnumMapper.GetStockSortOrder(sort))
            {
                case StockSort.Alphabetic:
                    tokensList = tokensList
                        .OrderBy(t => tokensDict[t])
                        .ToList();
                    break;

                case StockSort.Trendline:
                default:
                    var list = await _context.Trendline
                    .Where(t => t.tf == tf && t.hl == "h" && t.connects > 0)
                    .OrderByDescending(t => t.connects)
                    .Select(t => t.token)
                    .ToListAsync(); 
                  var categorySet = tokensList.ToHashSet();
                    tokensList = list
                    .Where(t => categorySet.Contains(t.ToString()))
                    .Select(t => t.ToString())
                    .ToList();
                    break;
            }
            var response = new
            {tokensList
            };

            return Ok(response);
        }   
      }
}