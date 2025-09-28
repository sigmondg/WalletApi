using System.Globalization;
using CurrencyUpdater.Data;
using CurrencyUpdater.Data.EntityModels;
using ECBGateway.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyUpdater.Services;

public interface ICurrencyService
{
    Task InserOrUpdateCurrencyRatesAsync(CurrencyRateResponse response);
    Task<int?> GetCurrencyIdByCodeAsync(string currencyCode);
    Task<List<string>> GetCurrencyCodesAsync();
    Task<decimal> ConvertAmountAsync(decimal walletBalance, int walletCurrencyCodeId, int targetCurrencyId);
    Task<decimal> GetCurrencyRateByCurrencyIdAsync(int targetCurrencyId);
    Task<string> GetCurrencyCodeByIdAsync(int walletCurrencyCodeId);
    Task<Dictionary<string, decimal>> GetAllCurrencyRatesAsync();
    Task<Dictionary<int, decimal>> GetAllCurrencyRatesByIdAsync();
    Task<Dictionary<int, string>> GetCurrencyIdToCodeMappingAsync();
}

public class CurrencyService : ICurrencyService
{
    private readonly CurrencyWalletDbContext _dbContext;

    public CurrencyService()
    {
    }

    public CurrencyService(CurrencyWalletDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InserOrUpdateCurrencyRatesAsync(CurrencyRateResponse response)
    {
        var newCurrencies = response.Rates
            .Where(kvp => !_dbContext.CurrencyCodes.Any(c => c.Code == kvp.Key))
            .Select(kvp => new CurrencyCodeEntity { Code = kvp.Key })
            .ToList();

        if (newCurrencies.Count > 0)
        {
            _dbContext.CurrencyCodes.AddRange(newCurrencies);
            await _dbContext.SaveChangesAsync();
        }

        var valuesClause = string.Join(", ",
            response.Rates.Select(r =>
                $"('{r.Key}', '{response.Time:yyyy-MM-dd}', {r.Value.ToString(CultureInfo.InvariantCulture)})"));

        var mergeSql = $@"MERGE INTO CurrencyRates AS target
USING (
    VALUES {valuesClause}
) AS source (CurrencyCode, RateDate, Rate)
    ON target.CurrencyCodeId = (
        SELECT Id FROM CurrencyCodes WHERE Code = source.CurrencyCode
    )
WHEN MATCHED THEN
    UPDATE SET target.Rate = source.Rate, target.RateDate = source.RateDate
WHEN NOT MATCHED THEN
    INSERT (CurrencyCodeId, RateDate, Rate)
    VALUES (
        (SELECT Id FROM CurrencyCodes WHERE Code = source.CurrencyCode),
        source.RateDate,
        source.Rate
    );";
        await _dbContext.Database.ExecuteSqlRawAsync(mergeSql);
    }

    public async Task<int?> GetCurrencyIdByCodeAsync(string currencyCode)
    {
        var currency = await _dbContext.CurrencyCodes
            .FirstOrDefaultAsync(c => c.Code.ToLower() == currencyCode.ToLower());

        return currency?.Id ?? null;
    }

    public async Task<string> GetCurrencyCodeByIdAsync(int currencyCodeId)
        => await _dbContext.CurrencyCodes.Where(c => c.Id == currencyCodeId)
            .Select(c => c.Code)
            .FirstAsync();

    public async Task<Dictionary<string, decimal>> GetAllCurrencyRatesAsync()
        => await _dbContext.CurrencyRates
            .Join(_dbContext.CurrencyCodes,
                rate => rate.CurrencyCodeId,
                currency => currency.Id,
                (rate, currency) => new { currency.Code, rate.Rate })
            .ToDictionaryAsync(x => x.Code, x => x.Rate);

    public async Task<List<string>> GetCurrencyCodesAsync()
        => await _dbContext.CurrencyCodes.Select(c => c.Code).ToListAsync();


    public async Task<decimal> ConvertAmountAsync(decimal amount, int fromCurrencyId, int toCurrencyId)
    {
        if (fromCurrencyId == toCurrencyId) return amount;

        var fromRate = await GetCurrencyRateByCurrencyIdAsync(fromCurrencyId);
        var toRate = await GetCurrencyRateByCurrencyIdAsync(toCurrencyId);

        var amountInBase = amount / fromRate;
        var convertedAmount = amountInBase * toRate;

        return convertedAmount;
    }

    public async Task<decimal> GetCurrencyRateByCurrencyIdAsync(int targetCurrencyId)
    {
        var rate = await _dbContext.CurrencyRates
            .Where(r => r.CurrencyCodeId == targetCurrencyId)
            .Select(r => r.Rate)
            .FirstOrDefaultAsync();

        return rate == 0 ? throw new ArgumentException($"Currency rate not found for currencyId {targetCurrencyId}") : rate;
    }

    public async Task<Dictionary<int, decimal>> GetAllCurrencyRatesByIdAsync()
        => await _dbContext.CurrencyRates
            .ToDictionaryAsync(x => x.CurrencyCodeId, x => x.Rate);

    public async Task<Dictionary<int, string>> GetCurrencyIdToCodeMappingAsync()
        => await _dbContext.CurrencyCodes.ToDictionaryAsync(c => c.Id, c => c.Code);


    public async Task<List<int>> GetCurrencyIdsAsync()
        => await _dbContext.CurrencyCodes.Select(c => c.Id).ToListAsync();
}