namespace TradingApp.Core.Entities
{
  public class Trendline
  {
    public int id { get; set; }
    public int token { get; set; }

    public DateTime starttime { get; set; }
    public DateTime endtime { get; set; }

    public double slope { get; set; }

    public double intercept { get; set; }

    public string hl { get; set; }

    public string tf { get; set; }

    public int index { get; set; }
    public int index1 { get; set; }
    public int index2 { get; set; }
    public int connects { get; set; }
    public int totalconnects { get; set; }
    public Trendline(int token, DateTime starttime, DateTime endtime, double slope, double intercept,
     string hl, string tf, int index, int index1, int index2, int connects, int totalconnects)
    {
      this.token = token;
      this.starttime = starttime;
      this.endtime = endtime;
      this.slope = slope;
      this.intercept = intercept;
      this.hl = hl;
      this.tf = tf;
      this.index = index;
      this.index1 = index1;
      this.index2 = index2;
      this.connects = connects;
      this.totalconnects = totalconnects;
    }
  }
}
