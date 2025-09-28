namespace ECBGateway.Models;

public class CurrencyRateResponse
{
    public DateTime Time { get; set; }
    public Dictionary<string, decimal> Rates { get; set; }
}