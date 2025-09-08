namespace TradingApp.Core.Interfaces
{
  public interface IService
  {
    Task ExecuteAsync(CancellationToken ct);
  }
}