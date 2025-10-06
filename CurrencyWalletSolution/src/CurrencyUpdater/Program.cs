using CurrencyUpdater.Data;
using CurrencyUpdater.Jobs;
using CurrencyUpdater.Services;
using ECBGateway.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<CurrencyWalletDbContext>((serviceProvider, options) =>
{
    var dbOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    options.UseSqlServer(dbOptions.ConnectionString);
});

builder.Services.AddMemoryCache();
builder.Services.AddGatewayService();
builder.Services.AddCachedCurrencyService();
builder.Services.AddJobs();

var configurator = new QuartzSchedulerConfigurator();
configurator.Configure(builder.Services);

var host = builder.Build();
await host.RunAsync();