using CurrencyUpdater.Services.Cache;
using ECBGateway.Models;

namespace CurrencyUpdater.Services;

public class CurrencyServiceCacheDecorator : ICurrencyService
{
    private readonly ICurrencyService _originalService;
    private readonly ICurrencyRateCache _cache;

    public CurrencyServiceCacheDecorator(
        ICurrencyService originalService,
        ICurrencyRateCache cache)
    {
        _originalService = originalService;
        _cache = cache;
    }

    public async Task<decimal> GetCurrencyRateByCurrencyIdAsync(int targetCurrencyId)
    {
        try
        {
            var rate = await _cache.GetRateByIdAsync(targetCurrencyId);

            if (rate.HasValue)
                return rate.Value;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error getting data from cache for {targetCurrencyId}: {e.Message} ");
        }

        return await _originalService.GetCurrencyRateByCurrencyIdAsync(targetCurrencyId);
    }

    public async Task<Dictionary<string, decimal>> GetAllCurrencyRatesAsync()
    {
        try
        {
            return await _cache.GetAllRatesbyCodeAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error getting all rates from cache, Message : {e.Message}");
            return await _originalService.GetAllCurrencyRatesAsync();
        }
    }

    public async Task<Dictionary<int, decimal>> GetAllCurrencyRatesByIdAsync()
    {
        try
        {
            return await _cache.GetAllRatesByIdAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error getting all rates by id from cache, Message : {e.Message}");
            return await _originalService.GetAllCurrencyRatesByIdAsync();
        }
    }

    public async Task<Dictionary<int, string>> GetCurrencyIdToCodeMappingAsync()
    {
        try
        {
            return await _cache.GetCurrencyIdToCodeMappingAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error getting currency ID to code mapping from cache, Message : {e.Message}");
            return await _originalService.GetCurrencyIdToCodeMappingAsync();
        }

    }

    public async Task<List<string>> GetCurrencyCodesAsync()
    {
        try
        {
            return await _cache.GetCurrencyCodes();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error getting currencyCodes from cache, Message : {e.Message}");
            return await _originalService.GetCurrencyCodesAsync();
        }
    }

    public async Task InserOrUpdateCurrencyRatesAsync(CurrencyRateResponse response)
        => await _originalService.InserOrUpdateCurrencyRatesAsync(response);

    public async Task<int?> GetCurrencyIdByCodeAsync(string currencyCode)
    {
        try
        {
            return await _cache.GetCurrencyIdByCodeAsync(currencyCode);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error getting CurrencyId by Code from cache, Message : {e.Message}");
            return await _originalService.GetCurrencyIdByCodeAsync(currencyCode);
        }
    }

    public async Task<decimal> ConvertAmountAsync(decimal amount, int fromCurrencyId, int toCurrencyId)
    {
        try
        {
            if (fromCurrencyId == toCurrencyId) return amount;

            var fromRate = await GetCurrencyRateByCurrencyIdAsync(fromCurrencyId);
            var toRate = await GetCurrencyRateByCurrencyIdAsync(toCurrencyId);

            var amountInBase = amount / fromRate;
            var convertedAmount = amountInBase * toRate;

            return convertedAmount;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error converting Amount from cache, Message : {e.Message}");
            return await _originalService.ConvertAmountAsync(amount, fromCurrencyId, toCurrencyId);
        }
    }

    public async Task<string> GetCurrencyCodeByIdAsync(int walletCurrencyCodeId)
        => await _originalService.GetCurrencyCodeByIdAsync(walletCurrencyCodeId);
}