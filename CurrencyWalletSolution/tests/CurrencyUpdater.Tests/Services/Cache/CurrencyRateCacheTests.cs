using CurrencyUpdater.Services;
using CurrencyUpdater.Services.Cache;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace CurrencyUpdater.Tests.Services.Cache;

public class CurrencyRateCacheTests
{
    [Fact]
    public async Task GetAllRatesByIdAsync_FirstCall_FetchesFromService()
    {
        var currencyServiceMock = new Mock<ICurrencyService>();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new CurrencyRateCache(memoryCache, currencyServiceMock.Object);
        var expectedRates = new Dictionary<int, decimal> { { 1, 1.5m } };

        currencyServiceMock.Setup(s => s.GetAllCurrencyRatesByIdAsync())
            .ReturnsAsync(expectedRates);

        var result = await cache.GetAllRatesByIdAsync();

        Assert.Equal(expectedRates, result);
        currencyServiceMock.Verify(s => s.GetAllCurrencyRatesByIdAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllRatesByIdAsync_SecondCall_ReturnsFromCache()
    {
        var currencyServiceMock = new Mock<ICurrencyService>();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new CurrencyRateCache(memoryCache, currencyServiceMock.Object);
        var expectedRates = new Dictionary<int, decimal> { { 1, 1.5m } };

        currencyServiceMock.Setup(s => s.GetAllCurrencyRatesByIdAsync())
            .ReturnsAsync(expectedRates);

        await cache.GetAllRatesByIdAsync(); // First call
        var result = await cache.GetAllRatesByIdAsync(); // Second call

        Assert.Equal(expectedRates, result);
        currencyServiceMock.Verify(s => s.GetAllCurrencyRatesByIdAsync(), Times.Once);
    }

    [Fact]
    public async Task GetRateByIdAsync_CachedData_ReturnsCorrectRate()
    {
        var currencyServiceMock = new Mock<ICurrencyService>();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new CurrencyRateCache(memoryCache, currencyServiceMock.Object);
        var rates = new Dictionary<int, decimal> { { 1, 1.5m } };

        currencyServiceMock.Setup(s => s.GetAllCurrencyRatesByIdAsync())
            .ReturnsAsync(rates);

        await cache.GetAllRatesByIdAsync(); // Cache the data
        var result = await cache.GetRateByIdAsync(1);

        Assert.Equal(1.5m, result);
    }

    [Fact]
    public async Task RefreshCacheAsync_ClearsAndReloadsCache()
    {
        // Arrange
        var currencyServiceMock = new Mock<ICurrencyService>();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new CurrencyRateCache(memoryCache, currencyServiceMock.Object);

        var initialRates = new Dictionary<int, decimal> { { 1, 1.5m } };
        var updatedRates = new Dictionary<int, decimal> { { 1, 2.0m } };

        var codeRates = new Dictionary<string, decimal> { { "USD", 1.5m } };
        var updatedCodeRates = new Dictionary<string, decimal> { { "USD", 2.0m } };

        currencyServiceMock.SetupSequence(s => s.GetAllCurrencyRatesByIdAsync())
            .ReturnsAsync(initialRates)
            .ReturnsAsync(updatedRates);

        currencyServiceMock.SetupSequence(s => s.GetAllCurrencyRatesAsync())
            .ReturnsAsync(codeRates)
            .ReturnsAsync(updatedCodeRates);

        await cache.GetAllRatesByIdAsync();
        await cache.RefreshCacheAsync();
        var result = await cache.GetAllRatesByIdAsync();

        Assert.Equal(updatedRates, result);
        currencyServiceMock.Verify(s => s.GetAllCurrencyRatesByIdAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task GetCurrencyIdByCode_ValidCode_ReturnsCorrectId()
    {
        var currencyServiceMock = new Mock<ICurrencyService>();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new CurrencyRateCache(memoryCache, currencyServiceMock.Object);
        
        var mapping = new Dictionary<int, string> 
        { 
            { 1, "USD" }, 
            { 2, "EUR" } 
        };

        currencyServiceMock.Setup(s => s.GetCurrencyIdToCodeMappingAsync())
            .ReturnsAsync(mapping);
        
        var result = await cache.GetCurrencyIdByCodeAsync("USD");
        
        Assert.Equal(1, result);
        currencyServiceMock.Verify(s => s.GetCurrencyIdToCodeMappingAsync(), Times.Once);
    }
}