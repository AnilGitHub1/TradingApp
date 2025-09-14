namespace TradingApp.Core.Entities
{
  public class FifteenTF : Candle
  {
    public FifteenTF(int token, DateTime time, double open, double high, double low, double close, double volume)
      : base(token, time, open, high, low, close, volume)
    {
    }
  }
}
