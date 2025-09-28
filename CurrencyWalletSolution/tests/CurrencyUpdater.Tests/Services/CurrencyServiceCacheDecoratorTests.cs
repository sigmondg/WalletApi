using CurrencyUpdater.Services;
using CurrencyUpdater.Services.Cache;
using Moq;

namespace CurrencyUpdater.Tests.Services;

public class CurrencyServiceCacheDecoratorTests
{
    private readonly Mock<ICurrencyService> _currencyServiceMock;
    private readonly Mock<ICurrencyRateCache> _currencyRateCache;
    private readonly CurrencyServiceCacheDecorator _decorator;

    public CurrencyServiceCacheDecoratorTests()
    {
        _currencyServiceMock = new Mock<ICurrencyService>();
        _currencyRateCache = new Mock<ICurrencyRateCache>();
        _decorator = new CurrencyServiceCacheDecorator(_currencyServiceMock.Object, _currencyRateCache.Object);
    }

    [Fact]
    public async Task GetCurrencyRateByCurrencyIdAsync_CacheHit_ReturnsFromCache()
    {
        const int currencyId = 1;
        const decimal expectedRate = 1.5m;

        var currencyServiceMock = new Mock<ICurrencyService>();
        var cacheMock = new Mock<ICurrencyRateCache>();
        var decorator = new CurrencyServiceCacheDecorator(currencyServiceMock.Object, cacheMock.Object);

        cacheMock.SetupSequence(x => x.GetRateByIdAsync(currencyId))
            .ReturnsAsync((decimal?)null)
            .ReturnsAsync(expectedRate);

        currencyServiceMock.Setup(x => x.GetCurrencyRateByCurrencyIdAsync(currencyId))
            .ReturnsAsync(expectedRate);

        var firstCall = await decorator.GetCurrencyRateByCurrencyIdAsync(currencyId);
        var secondCall = await decorator.GetCurrencyRateByCurrencyIdAsync(currencyId);

        Assert.Equal(expectedRate, firstCall);
        Assert.Equal(expectedRate, secondCall);

        currencyServiceMock.Verify(x => x.GetCurrencyRateByCurrencyIdAsync(currencyId), Times.Once);
        cacheMock.Verify(x => x.GetRateByIdAsync(currencyId), Times.Exactly(2));
    }

    [Fact]
    public async Task GetCurrencyRateByCurrencyIdAsync_CacheMiss_FallsBackToOriginalService()
    {
        const int currencyId = 1;
        const decimal expectedRate = 1.5m;

        _currencyServiceMock
            .Setup(x => x.GetCurrencyRateByCurrencyIdAsync(currencyId))
            .ReturnsAsync(expectedRate);

        var result = await _decorator.GetCurrencyRateByCurrencyIdAsync(currencyId);

        Assert.Equal(expectedRate, result);
        _currencyServiceMock.Verify(x => x.GetCurrencyRateByCurrencyIdAsync(currencyId), Times.Once);
    }

    [Fact]
    public async Task ConvertAmountAsync_UsesDecoratedMethods()
    {
        const decimal amount = 100m;
        const int fromCurrencyId = 1;
        const int toCurrencyId = 2;
        const decimal expectedResult = 150m;

        _currencyServiceMock
            .Setup(x => x.ConvertAmountAsync(amount, fromCurrencyId, toCurrencyId))
            .ReturnsAsync(expectedResult);

        var result = await _decorator.ConvertAmountAsync(amount, fromCurrencyId, toCurrencyId);

        Assert.Equal(expectedResult, result);
        _currencyServiceMock.Verify(x => x.ConvertAmountAsync(amount, fromCurrencyId, toCurrencyId), Times.Once);
    }
}