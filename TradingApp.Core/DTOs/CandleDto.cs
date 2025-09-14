namespace TradingApp.Core.DTOs
{
  public class CandleDto
  {
    public int token;
    public DateTime time;
    public double open;
    public double high;
    public double low;
    public double close;
    public double volume;

    public CandleDto(int token, DateTime time, double open, double high, double low, double close, double volume)
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