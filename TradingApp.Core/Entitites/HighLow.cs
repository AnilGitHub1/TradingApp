namespace TradingApp.Core.Entities
{
  public class HighLow : Candle
  {
    public int? index { get; set; }
    public string? hl { get; set; }
    public string? tf { get; set; }
    public HighLow(int index, string hl, string tf, int token, DateTime time, double open, double high, double low, double close, double volume)
      : base(token, time, open, high, low, close, volume)
    {
      this.index = index;
      this.hl = hl;
      this.tf = tf;
    }
    public HighLow(int index, string hl, string tf, Candle candle)
      : base(candle.token, candle.time, candle.open, candle.high, candle.low, candle.close, candle.volume)
    {
      this.index = index;
      this.hl = hl;
      this.tf = tf;
    }
  }
}
