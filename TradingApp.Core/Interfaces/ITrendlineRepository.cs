using TradingApp.Core.Entities;
using YourProject.Interfaces;

namespace TradingApp.Core.Interfaces
{
    public interface ITrendlineRepository : IRepository<Trendline>
    {
        Task<IList<Trendline>> GetTrendlineAsync(int token, int page, int pageSize);
        Task<IList<Trendline>> GetTrendlineAsync(int token, int limit);
        Task<IList<Trendline>> GetAllTrendlineAsync(int token);
        Task<IList<Trendline>> GetAllTrendlineAsync(int token, DateTime from);
        Task<IList<Trendline>> GetAllTrendlineAsync(int token, DateTime from, DateTime to);
        Task AddTrendlineAsync(Trendline Trendline);
        Task AddTrendlineAsync(IList<Trendline> Trendline);
        Task UpdateTrendlineAsync(Trendline Trendline);
        Task UpdateTrendlinesAsync(List<Trendline> Trendlines);
        Task DeleteTrendlineAsync(int id);
        Task DeleteTrendlineAsync(Trendline entity);
        Task<int> DeleteTrendlineDataAsync(int token);
    }
}