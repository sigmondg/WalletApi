using CurrencyUpdater.Services.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyUpdater.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCachedCurrencyService(this IServiceCollection services)
    {
        services.AddScoped<CurrencyService>();
        services.AddScoped<ICurrencyService>(provider => provider.GetRequiredService<CurrencyService>());
        services.AddScoped<ICurrencyRateCache>(provider =>
            new CurrencyRateCache(
                provider.GetRequiredService<IMemoryCache>(),
                provider.GetRequiredService<CurrencyService>()
            ));
        
        services.Decorate<ICurrencyService, CurrencyServiceCacheDecorator>();
        
        return services;
    }
}