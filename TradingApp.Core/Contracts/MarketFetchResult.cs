namespace TradingApp.Core.Contracts
{
    public record CandleRow(int Token, DateTime Time, double Open, double High, double Low, double Close, double Volume);
    public record MarketFetchResult(string Symbol, int Token, List<CandleRow> Rows);
}


