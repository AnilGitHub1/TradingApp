namespace TradingApp.API.Models;

public class DailyTFData
{
    public int? id { get; set; }

    public int? token { get; set; }

    public DateTime? time { get; set; }

    public double? open { get; set; }

    public double? high { get; set; }

    public double? low { get; set; }

    public double? close { get; set; }

    public int? volume { get; set; }
}
