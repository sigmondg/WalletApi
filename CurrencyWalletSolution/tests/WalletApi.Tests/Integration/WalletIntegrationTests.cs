using CurrencyUpdater.Data;
using CurrencyUpdater.Data.EntityModels;
using CurrencyUpdater.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WalletApi.Controllers;
using WalletApi.Models;
using WalletApi.Services.Wallet;

namespace WalletApi.Tests.Integration;

public class WalletIntegrationTests
{ 
    private readonly Mock<ICurrencyService> _currencyServiceMock = new();
    private readonly Mock<BalanceStrategyFactory> _strategyFactoryMock = new();
    
    private CurrencyWalletDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<CurrencyWalletDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new CurrencyWalletDbContext(options);
    }
    
    [Fact]
    public async Task CreateWallet_AdjustBalance_GetBalance_FullWorkflow()
    {
        await using var context = CreateInMemoryDbContext();
        var controller = new WalletController(context, _currencyServiceMock.Object, _strategyFactoryMock.Object);
        
        _currencyServiceMock.Setup(s => s.GetCurrencyIdByCodeAsync("USD")).ReturnsAsync(1);
        _currencyServiceMock.Setup(s => s.GetCurrencyCodeByIdAsync(1)).ReturnsAsync("USD");
        
        var createRequest = new CreateWalletRequest { Balance = 100, CurrencyCode = "USD" };
        var createResult = await controller.CreateWallet(createRequest);
        var createResponse = Assert.IsType<OkObjectResult>(createResult);
        var walletResponse = Assert.IsType<CreateWalletResponse>(createResponse.Value);
        
        var strategyMock = new Mock<IBalanceStrategy>();
        strategyMock.Setup(s => s.AdjustBalance(It.IsAny<WalletEntity>(), 50)).Returns(150);
        _strategyFactoryMock.Setup(f => f.GetStrategy("add")).Returns(strategyMock.Object);

        var adjustResult = await controller.AdjustBalance(walletResponse.Id, 50, "USD", "add");
        var adjustResponse = Assert.IsType<OkObjectResult>(adjustResult);
        var adjustBalanceResponse = Assert.IsType<AdjustBalanceResponse>(adjustResponse.Value);
        
        Assert.Equal(150, adjustBalanceResponse.Balance);
        
        var balanceResult = await controller.GetBalance(walletResponse.Id, null);
        var balanceResponse = Assert.IsType<OkObjectResult>(balanceResult);
        var getBalanceResponse = Assert.IsType<GetBalanceResponse>(balanceResponse.Value);
        
        Assert.NotNull(getBalanceResponse);
        Assert.Equal(150, getBalanceResponse.Balance);
        Assert.Equal("USD", getBalanceResponse.CurrencyCode);
        Assert.Equal(walletResponse.Id, getBalanceResponse.Id);
        
        _currencyServiceMock.Verify(s => s.GetCurrencyIdByCodeAsync("USD"), Times.AtLeastOnce);
        _currencyServiceMock.Verify(s => s.GetCurrencyCodeByIdAsync(1), Times.AtLeastOnce);
        strategyMock.Verify(s => s.AdjustBalance(It.IsAny<WalletEntity>(), 50), Times.Once);
    }
    
    [Fact]
    public async Task CurrencyConversion_RealData_WorksCorrectly()
    {
        await using var context = CreateInMemoryDbContext();
        var wallet = new WalletEntity { Id = Guid.NewGuid(), Balance = 100, CurrencyCodeId = 1 };
        context.Wallets.Add(wallet);
        await context.SaveChangesAsync();

        _currencyServiceMock.Setup(s => s.GetCurrencyIdByCodeAsync("EUR")).ReturnsAsync(2);
        _currencyServiceMock.Setup(s => s.ConvertAmountAsync(100, 1, 2)).ReturnsAsync(85m);

        var controller = new WalletController(context, _currencyServiceMock.Object, _strategyFactoryMock.Object);

        var result = await controller.GetBalance(wallet.Id, "EUR");
        var balance = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GetBalanceResponse>(balance.Value);

        Assert.Equal(85m, response.Balance);
        _currencyServiceMock.Verify(s => s.ConvertAmountAsync(100, 1, 2), Times.Once);
    }
}