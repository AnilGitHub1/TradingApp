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

        public string? hl { get; set; }

        public string? tf { get; set; }

        public int index { get; set; }
        public int index1 { get; set; }
        public int index2 { get; set; }
        public int connects { get; set; }
        public int totalconnects { get; set; }
    }
}
