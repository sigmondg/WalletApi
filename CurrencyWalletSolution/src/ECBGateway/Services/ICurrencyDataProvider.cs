using ECBGateway.Models;

namespace ECBGateway.Services;

public interface ICurrencyDataProvider
{
    string ProviderName { get; }
    Task<CurrencyRateResponse> GetLatestCurrencyRates();
}