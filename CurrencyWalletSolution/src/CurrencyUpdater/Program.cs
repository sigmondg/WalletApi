using CurrencyUpdater.Data;
using CurrencyUpdater.Jobs;
using CurrencyUpdater.Services;
using ECBGateway.Services;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<CurrencyWalletDbContext>(options => { options.UseSqlServer(builder.Configuration.GetConnectionString("CurrencyWalletDb")); });

builder.Services.AddMemoryCache();
builder.Services.AddGatewayService();
builder.Services.AddCachedCurrencyService();
builder.Services.AddJobs();

var configurator = new QuartzSchedulerConfigurator();
configurator.Configure(builder.Services);

var host = builder.Build();
await host.RunAsync();