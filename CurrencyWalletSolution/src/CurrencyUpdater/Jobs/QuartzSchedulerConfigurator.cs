using Quartz;
namespace CurrencyUpdater.Jobs;

public interface IQuartzSchedulerConfigurator
{
    void Configure(IServiceCollection services);
}

public class QuartzSchedulerConfigurator : IQuartzSchedulerConfigurator
{
    public void Configure(IServiceCollection services)
    {
        services.AddQuartz(quartz =>
        {
            quartz.AddJob<CurrencyRateUpdateJob>(options => options.WithIdentity("CurrencyRateUpdateJob"));

            quartz.AddTrigger(options => options
                .ForJob("CurrencyRateUpdateJob")
                .WithIdentity("CurrencyRateUpdateJob-Trigger")
                .WithSimpleSchedule(schedule => schedule
                    .WithIntervalInHours(12)
                    .RepeatForever())
                .StartNow());
        });
        
        services.AddQuartzHostedService(quartz => quartz.WaitForJobsToComplete = true);
    }
}