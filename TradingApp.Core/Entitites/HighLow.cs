namespace TradingApp.Core.Entities
{
    public class HighLow
    {
        public int id { get; set; }
        public int token { get; set; }
        public DateTime time { get; set; }
        public double open { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double close { get; set; }
        public double volume { get; set; }
        public string? hl { get; set; }
        public string? tf { get; set; }    
    }
}
