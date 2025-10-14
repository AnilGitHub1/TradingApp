namespace TradingApp.Core.Entities
{
  public class HighLow
  {
    public int id { get; set; }
    public string hl { get; set; }
    public string tf { get; set; }
    public int token { get; set; }
    public DateTime time { get; set; }
    public HighLow(string hl, string tf, Candle candle)
    {
      this.hl = hl;
      this.tf = tf;
      this.token = candle.token;
      this.time = candle.time;
    }
  }
}
