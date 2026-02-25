public class DhanScanSettings
{
    public string BaseUrl { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public Dictionary<string, string> Headers { get; set; } = new();
    public object Payload { get; set; } = new();
}