namespace TradingApp.Core.Entities
{
  public class DailyTF : Candle
  {
    public DailyTF(int token, DateTime time, double open, double high, double low, double close, double volume)
      : base(token, time, open, high, low, close, volume)
    {
    }
  }
}
