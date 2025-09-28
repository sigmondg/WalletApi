using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Options;
using Shared.Validators;

namespace Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName))
            .AddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>();

        services.Configure<IpRateLimitingOptions>(configuration.GetSection(IpRateLimitingOptions.SectionName))
            .AddSingleton<IValidateOptions<IpRateLimitingOptions>, IpRateLimitingValidator>();

        return services;
    }
}