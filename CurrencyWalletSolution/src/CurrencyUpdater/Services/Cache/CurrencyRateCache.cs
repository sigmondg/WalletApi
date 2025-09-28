using Microsoft.Extensions.Caching.Memory;

namespace CurrencyUpdater.Services.Cache;

public interface ICurrencyRateCache
{
    Task<Dictionary<string, decimal>> GetAllRatesbyCodeAsync();
    Task<Dictionary<int, decimal>> GetAllRatesByIdAsync();
    Task RefreshCacheAsync();
    Task<decimal?> GetRateByIdAsync(int currencyId);
    Task<List<string>> GetCurrencyCodes();
    Task<List<int>> GetCurrencyIds();
    Task<int?> GetCurrencyIdByCodeAsync(string currencyCode);
    Task<Dictionary<int, string>> GetCurrencyIdToCodeMappingAsync();
}

public class CurrencyRateCache : ICurrencyRateCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly ICurrencyService _originalService;

    public CurrencyRateCache(
        IMemoryCache memoryCache,
        ICurrencyService originalService)
    {
        _memoryCache = memoryCache;
        _originalService = originalService;
    }

    public async Task<Dictionary<string, decimal>> GetAllRatesbyCodeAsync()
    {
        if (_memoryCache.TryGetValue(CacheKeys.CurrencyRates, out Dictionary<string, decimal>? currencyRateCache))
        {
            return currencyRateCache!;
        }

        var rates = await _originalService.GetAllCurrencyRatesAsync();
        _memoryCache.Set(CacheKeys.CurrencyRates, rates);

        return rates;
    }

    public async Task<Dictionary<int, decimal>> GetAllRatesByIdAsync()
    {
        if (_memoryCache.TryGetValue(CacheKeys.CurrencyRatesById, out Dictionary<int, decimal>? currencyRateByIdCache))
            return currencyRateByIdCache!;

        var rates = await _originalService.GetAllCurrencyRatesByIdAsync();
        _memoryCache.Set(CacheKeys.CurrencyRatesById, rates);

        return rates;
    }

    public async Task RefreshCacheAsync()
    {
        _memoryCache.Remove(CacheKeys.CurrencyRatesById);
        _memoryCache.Remove(CacheKeys.CurrencyIdMapping);

        await GetAllRatesByIdAsync();
        await GetCurrencyIdToCodeMappingAsync();
    }

    public async Task<decimal?> GetRateByIdAsync(int currencyId)
    {
        var allRates = await GetAllRatesByIdAsync();
        return allRates.TryGetValue(currencyId, out var rate) ? rate : null;
    }

    public async Task<List<int>> GetCurrencyIds()
    {
        var rates = await GetAllRatesByIdAsync();
        return rates.Keys.ToList();
    }

    public async Task<int?> GetCurrencyIdByCodeAsync(string currencyCode)
    {
        var mappings = await GetCurrencyIdToCodeMappingAsync();
        var kvp = mappings.FirstOrDefault(m => m.Value.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));
        return kvp.Key == 0 ? null : kvp.Key;
    }

    public async Task<Dictionary<int, string>> GetCurrencyIdToCodeMappingAsync()
    {
        if (_memoryCache.TryGetValue(CacheKeys.CurrencyIdMapping, out Dictionary<int, string>? mappingCache))
            return mappingCache!;
        
        var mapping = await _originalService.GetCurrencyIdToCodeMappingAsync();
        _memoryCache.Set(CacheKeys.CurrencyIdMapping, mapping);
        return mapping;
    }

    public async Task<List<string>> GetCurrencyCodes()
    {
        var rates = await GetAllRatesbyCodeAsync();
        return rates.Keys.ToList();
    }
}