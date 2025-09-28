using CurrencyUpdater.Data;
using CurrencyUpdater.Services;
using CurrencyUpdater.Services.Cache;
using ECBGateway.Services;
using Quartz;

namespace CurrencyUpdater.Jobs;

public class CurrencyRateUpdateJob : IJob
{
    private readonly ICurrencyDataProvider _currencyDataGateway;
    private readonly CurrencyWalletDbContext _dbContext;
    private readonly ICurrencyService _currencyService;
    private readonly ICurrencyRateCache _currencyRateCache;

    public CurrencyRateUpdateJob(
        ICurrencyDataProvider currencyDataGateway,
        CurrencyWalletDbContext dbContext,
        ICurrencyService currencyService,
        ICurrencyRateCache currencyRateCache)
    {
        _currencyDataGateway = currencyDataGateway;
        _dbContext = dbContext;
        _currencyService = currencyService;
        _currencyRateCache = currencyRateCache;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var currencyRateResponse = await _currencyDataGateway.GetLatestCurrencyRates();
        await _currencyService.InserOrUpdateCurrencyRatesAsync(currencyRateResponse);
        await _currencyRateCache.RefreshCacheAsync();
    }
}