using CurrencyUpdater.Data;
using CurrencyUpdater.Jobs;
using CurrencyUpdater.Services;
using CurrencyUpdater.Services.Cache;
using ECBGateway.Models;
using ECBGateway.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Quartz;

namespace CurrencyUpdater.Tests.Jobs;

public class CurrencyRateUpdateJobTests
{
    private readonly Mock<ICurrencyDataProvider> _gatewayMock;
    private readonly Mock<ICurrencyService> _currencyServiceMock;
    private readonly Mock<ICurrencyRateCache> _rateCacheMock;
    private readonly CurrencyWalletDbContext _dbContext;
    private readonly CurrencyRateUpdateJob _job;

    public CurrencyRateUpdateJobTests()
    {
        _gatewayMock = new Mock<ICurrencyDataProvider>();
        _currencyServiceMock = new Mock<ICurrencyService>();
        _rateCacheMock = new Mock<ICurrencyRateCache>();

        var options = new DbContextOptionsBuilder<CurrencyWalletDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new CurrencyWalletDbContext(options);

        _job = new CurrencyRateUpdateJob(
            _gatewayMock.Object,
            _dbContext,
            _currencyServiceMock.Object,
            _rateCacheMock.Object);
    }

    [Fact]
    public async Task Execute_SuccessfulUpdate_CallsAllServices()
    {
        // Arrange
        var response = new CurrencyRateResponse
        { Time = DateTime.UtcNow,
          Rates = new Dictionary<string, decimal> { { "USD", 1.2m } } };

        _gatewayMock.Setup(x => x.GetLatestCurrencyRates())
            .ReturnsAsync(response);

        _currencyServiceMock.Setup(x => x.InserOrUpdateCurrencyRatesAsync(It.IsAny<CurrencyRateResponse>()))
            .Returns(Task.CompletedTask);

        _rateCacheMock.Setup(x => x.RefreshCacheAsync())
            .Returns(Task.CompletedTask);

        var jobExecutionContext = new Mock<IJobExecutionContext>();

        // Act
        await _job.Execute(jobExecutionContext.Object);

        // Assert
        _gatewayMock.Verify(x => x.GetLatestCurrencyRates(), Times.Once);
        _currencyServiceMock.Verify(x => x.InserOrUpdateCurrencyRatesAsync(response), Times.Once);
        _rateCacheMock.Verify(x => x.RefreshCacheAsync(), Times.Once);
    }

    [Fact]
    public async Task Execute_GatewayFails_HandlesGracefully()
    {
        _gatewayMock.Setup(x => x.GetLatestCurrencyRates())
            .ThrowsAsync(new Exception("Unexpected exception"));

        var jobExecutionContext = new Mock<IJobExecutionContext>();
        var exception = await Record.ExceptionAsync(() => _job.Execute(jobExecutionContext.Object));

        Assert.NotNull(exception);

        _gatewayMock.Verify(x => x.GetLatestCurrencyRates(), Times.Once);
        _currencyServiceMock.Verify(x => x.InserOrUpdateCurrencyRatesAsync(It.IsAny<CurrencyRateResponse>()), Times.Never);
        _rateCacheMock.Verify(x => x.RefreshCacheAsync(), Times.Never);
    }

    [Fact]
    public async Task Execute_DatabaseFails_HandlesGracefully()
    {
        var response = new CurrencyRateResponse
        { Time = DateTime.UtcNow,
          Rates = new Dictionary<string, decimal> { { "USD", 1.2m } } };

        _gatewayMock.Setup(x => x.GetLatestCurrencyRates())
            .ReturnsAsync(response);

        _currencyServiceMock.Setup(x => x.InserOrUpdateCurrencyRatesAsync(It.IsAny<CurrencyRateResponse>()))
            .ThrowsAsync(new Exception("Database error"));

        var jobExecutionContext = new Mock<IJobExecutionContext>();
        var exception = await Record.ExceptionAsync(() => _job.Execute(jobExecutionContext.Object));
        Assert.NotNull(exception);

        _gatewayMock.Verify(x => x.GetLatestCurrencyRates(), Times.Once);
        _currencyServiceMock.Verify(x => x.InserOrUpdateCurrencyRatesAsync(response), Times.Once);
        _rateCacheMock.Verify(x => x.RefreshCacheAsync(), Times.Never);
    }

    [Fact]
    public async Task Execute_CacheRefreshFails_StillUpdatesDatabase()
    {
        var response = new CurrencyRateResponse
        { Time = DateTime.UtcNow,
          Rates = new Dictionary<string, decimal> { { "USD", 1.2m } } };

        _gatewayMock.Setup(x => x.GetLatestCurrencyRates())
            .ReturnsAsync(response);

        _currencyServiceMock.Setup(x => x.InserOrUpdateCurrencyRatesAsync(It.IsAny<CurrencyRateResponse>()))
            .Returns(Task.CompletedTask);

        _rateCacheMock.Setup(x => x.RefreshCacheAsync())
            .ThrowsAsync(new Exception("Cache refresh error"));

        var jobExecutionContext = new Mock<IJobExecutionContext>();

        var exception = await Record.ExceptionAsync(() => _job.Execute(jobExecutionContext.Object));
        Assert.NotNull(exception);

        _gatewayMock.Verify(x => x.GetLatestCurrencyRates(), Times.Once);
        _currencyServiceMock.Verify(x => x.InserOrUpdateCurrencyRatesAsync(response), Times.Once);
        _rateCacheMock.Verify(x => x.RefreshCacheAsync(), Times.Once);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}