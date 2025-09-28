using Quartz;

namespace CurrencyUpdater.Jobs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJobs(this IServiceCollection services)
    {
        services.AddScoped<CurrencyRateUpdateJob>();
        services.AddSingleton<IQuartzSchedulerConfigurator, QuartzSchedulerConfigurator>();

        return services;
    }
}