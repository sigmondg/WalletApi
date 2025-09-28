using CurrencyUpdater.Data;
using CurrencyUpdater.Data.EntityModels;
using CurrencyUpdater.Services;
using Microsoft.EntityFrameworkCore;

namespace CurrencyUpdater.Tests.Services;

public class CurrencyServicesTests
{
    private CurrencyWalletDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<CurrencyWalletDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CurrencyWalletDbContext(options);
    }

    [Fact]
    public async Task GetCurrencyIdByCodeAsync_ValidCode_ReturnsCorrectId()
    {
        await using var context = CreateInMemoryDbContext();
        context.CurrencyCodes.AddRange(new List<CurrencyCodeEntity>
        { new() { Id = 1, Code = "USD" },
          new() { Id = 2, Code = "EUR" } });
        await context.SaveChangesAsync();

        var service = new CurrencyService(context);
        
        var result = await service.GetCurrencyIdByCodeAsync("USD");

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetCurrencyIdByCodeAsync_InvalidCode_ReturnsNull()
    {
        await using var context = CreateInMemoryDbContext();
        context.CurrencyCodes.Add(new CurrencyCodeEntity { Id = 1, Code = "USD" });
        await context.SaveChangesAsync();

        var service = new CurrencyService(context);

        var result = await service.GetCurrencyIdByCodeAsync("XYZ");
        
        Assert.Null(result);
    }

    [Fact]
    public async Task ConvertAmountAsync_SameCurrency_ReturnsOriginalAmount()
    {
        await using var context = CreateInMemoryDbContext();
        context.CurrencyRates.Add(new CurrencyRateEntity { CurrencyCodeId = 1, Rate = 1.2m });
        await context.SaveChangesAsync();

        var service = new CurrencyService(context);

        var result = await service.ConvertAmountAsync(100m, 1, 1);

        Assert.Equal(100m, result);
    }

    [Fact]
    public async Task ConvertAmountAsync_DifferentCurrencies_ReturnsConvertedAmount()
    {
        await using var context = CreateInMemoryDbContext();
        context.CurrencyRates.AddRange(new List<CurrencyRateEntity>
        { new() { CurrencyCodeId = 1, Rate = 1.2m }, // USD
          new() { CurrencyCodeId = 2, Rate = 0.8m } // EUR
        });
        await context.SaveChangesAsync();

        var service = new CurrencyService(context);

        var result = await service.ConvertAmountAsync(100m, 1, 2);
        
        Assert.Equal(66.67m, Math.Round(result, 2));
    }

    [Fact]
    public async Task GetCurrencyRateByCurrencyIdAsync_ExistingId_ReturnsRate()
    {
        await using var context = CreateInMemoryDbContext();
        context.CurrencyRates.Add(new CurrencyRateEntity { CurrencyCodeId = 1, Rate = 1.5m });
        await context.SaveChangesAsync();

        var service = new CurrencyService(context);
        var result = await service.GetCurrencyRateByCurrencyIdAsync(1);
        
        Assert.Equal(1.5m, result);
    }

    [Fact]
    public async Task GetCurrencyRateByCurrencyIdAsync_NonExistentId_ThrowsArgumentException()
    {
        await using var context = CreateInMemoryDbContext();
        var service = new CurrencyService(context);
        
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetCurrencyRateByCurrencyIdAsync(999));
    }

    [Fact]
    public async Task GetAllCurrencyRatesAsync_WithData_ReturnsCorrectMapping()
    {
        await using var context = CreateInMemoryDbContext();

        context.CurrencyCodes.AddRange(new List<CurrencyCodeEntity>
        { new() { Id = 1, Code = "USD" },
          new() { Id = 2, Code = "EUR" } });

        context.CurrencyRates.AddRange(new List<CurrencyRateEntity>
        { new() { CurrencyCodeId = 1, Rate = 1.2m },
          new() { CurrencyCodeId = 2, Rate = 0.8m } });

        await context.SaveChangesAsync();

        var service = new CurrencyService(context);
        
        var result = await service.GetAllCurrencyRatesAsync();
        
        Assert.Equal(2, result.Count);
        Assert.Equal(1.2m, result["USD"]);
        Assert.Equal(0.8m, result["EUR"]);
    }
}