using AspNetCoreRateLimit;
using CurrencyUpdater.Data;
using CurrencyUpdater.Jobs;
using CurrencyUpdater.Services;
using ECBGateway.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared;
using Shared.Options;
using WalletApi.Services.Kafka;
using WalletApi.Services.Wallet;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSharedOptions(builder.Configuration);


// Add services to the container.
builder.Services.AddDbContext<CurrencyWalletDbContext>((serviceProvider, options) =>
{
    var dbOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    options.UseSqlServer(dbOptions.ConnectionString);
});

builder.Services.AddOptions();
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();


builder.Services.AddCachedCurrencyService();
builder.Services.AddWalletService();
builder.Services.AddJobs();
builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddHostedService<KafkaConsumerService>();
builder.Services.AddGatewayService();

var configurator = new QuartzSchedulerConfigurator();
configurator.Configure(builder.Services);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseIpRateLimiting();
app.MapControllers();
app.Run();