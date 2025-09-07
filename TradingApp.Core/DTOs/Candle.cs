namespace TradingApp.Core.DTOs
{
    public record Candle
    (
        int token,
        DateTime time ,
        double open ,
        double high ,
        double low ,
        double close ,
        double volume
    );
}