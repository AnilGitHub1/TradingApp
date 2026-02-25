public class DhanScanDto
{
    public string? DispSym { get; set; }
    public decimal DivYeild {get; set;}
    public string? Isin { get; set; }
    public string? Inst { get; set; }
    public string? Exch { get; set; }
    public string? Seg { get; set; }
    public string? Sym { get; set; }
    public decimal Mcap { get; set; }
    public decimal Pe { get; set; }
    public decimal Eps { get; set; }
    public decimal TickSize {get; set;}
    public int LotSize { get; set; }
}