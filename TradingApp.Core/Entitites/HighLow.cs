namespace TradingApp.Core.Entities
{
  public class HighLow
  {
    public int id { get; set; }
    public int? index { get; set; }
    public string? hl { get; set; }
    public string? tf { get; set; }
    public int token { get; set; }
    public DateTime time { get; set; }
    public double open { get; set; }
    public double high { get; set; }
    public double low { get; set; }
    public double close { get; set; }
    public double volume { get; set; }
    public HighLow() { }
    public HighLow(int index, string hl, string tf, Candle candle)
    {
      this.index = index;
      this.hl = hl;
      this.tf = tf;
      this.token = candle.token;
      this.time = candle.time;
      this.open = candle.open;
      this.close = candle.close;
      this.high = candle.high;
      this.low = candle.low;
      this.volume = candle.volume;
    }
  }
}
