namespace TradingApp.Core.Entities
{
  public class FifteenTF : Candle
  {
    public FifteenTF(int token, DateTime time, double open, double high, double low, double close, int volume)
      : base(token, time, open, high, low, close, volume)
    {
    }
  }
}
