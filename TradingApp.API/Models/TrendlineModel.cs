namespace TradingApp.API.Models;

public class TrendlineModel
{
    public int? token { get; set; }

    public DateTime? startTime { get; set; }
    public DateTime? endTime { get; set; }

    public double? slope { get; set; }

    public double? intercept { get; set; }

    public string? hl { get; set; }

    public string? tf { get; set; }

    public int? index { get; set; }
    public int? index1 { get; set; }
    public int? index2 { get; set; }
    public int? connects { get; set; }
    public int? totalConnects { get; set; }
}
