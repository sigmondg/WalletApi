using Microsoft.Extensions.DependencyInjection;

namespace ECBGateway.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGatewayService(this IServiceCollection services)
    {
        services.AddSingleton<HttpClient>();
        services.AddSingleton<EcbCurrencyDataProvider>();
        services.AddSingleton<ICurrencyDataProvider, EcbCurrencyDataProvider>();

        return services;
    }
}