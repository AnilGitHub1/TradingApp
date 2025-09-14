namespace TradingApp.Core.Entities
{
  public class Candle
  {
    public int id { get; set; }
    public int token { get; set; }
    public DateTime time { get; set; }
    public double open { get; set; }
    public double high { get; set; }
    public double low { get; set; }
    public double close { get; set; }
    public double volume { get; set; }
  public Candle(int token, DateTime time, double open, double high, double low, double close, double volume)
    {
      this.token = token;
      this.time = time;
      this.open = open;
      this.close = close;
      this.high = high;
      this.low = low;
      this.volume = volume;
    }
  }
}