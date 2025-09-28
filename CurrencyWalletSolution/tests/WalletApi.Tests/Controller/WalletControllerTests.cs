using CurrencyUpdater.Data;
using CurrencyUpdater.Data.EntityModels;
using CurrencyUpdater.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WalletApi.Controllers;
using WalletApi.Models;
using WalletApi.Services.Wallet;

namespace WalletApi.Tests.Controller;

public class WalletControllerTests
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
    public async Task AdjustBalance_WalletNotFound_ReturnsNotFound()
    {
        await using var context = CreateInMemoryDbContext();

        var controller = new WalletController(context, _currencyServiceMock.Object, _strategyFactoryMock.Object);

        var walletId = Guid.NewGuid();
        var result = await controller.AdjustBalance(walletId, 10, "USD", "add");

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateWallet_Valid_ReturnsOkAndWalletId()
    {
        await using var context = CreateInMemoryDbContext();
        var controller = new WalletController(context, _currencyServiceMock.Object, _strategyFactoryMock.Object);

        _currencyServiceMock
            .Setup(s => s.GetCurrencyIdByCodeAsync("USD"))
            .ReturnsAsync(1);

        var walletModel = new CreateWalletRequest { Balance = 100, CurrencyCode = "USD" };

        var result = await controller.CreateWallet(walletModel);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var walletResponse = Assert.IsType<CreateWalletResponse>(okResult.Value!);

        var walletInDb = await context.Wallets.FindAsync(walletResponse.Id);
        Assert.NotNull(walletInDb);
        Assert.Equal(100, walletInDb.Balance);
        Assert.Equal(1, walletInDb.CurrencyCodeId);
        Assert.Equal(walletInDb.Id, walletResponse.Id);
    }

    [Fact]
    public async Task RetrieveWalletBalance_WalletNotFound_ReturnsNotFound()
    {
        await using var context = CreateInMemoryDbContext();
        var controller = new WalletController(context, _currencyServiceMock.Object, _strategyFactoryMock.Object);

        var walletId = Guid.NewGuid();
        var result = await controller.GetBalance(walletId, "USD");

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task RetrieveWalletBalance_ConvertsCurrency_Success()
    {
        await using var context = CreateInMemoryDbContext();

        var wallet = new WalletEntity { Id = Guid.NewGuid(), Balance = 100, CurrencyCodeId = 1 };
        context.Wallets.Add(wallet);
        await context.SaveChangesAsync();

        _currencyServiceMock.Setup(s => s.GetCurrencyIdByCodeAsync("INR")).ReturnsAsync(22);
        _currencyServiceMock.Setup(s => s.ConvertAmountAsync(100, 1, 22))
            .ReturnsAsync(90m);

        var controller = new WalletController(context, _currencyServiceMock.Object, _strategyFactoryMock.Object);

        var result = await controller.GetBalance(wallet.Id, "INR");

        var ok = Assert.IsType<OkObjectResult>(result);
        var walletResponse = Assert.IsType<GetBalanceResponse>(ok.Value!);
        Assert.Equal(90m, walletResponse.Balance);
    }

    [Fact]
    public async Task AdjustBalance_InvalidCurrency_ReturnsBadRequest()
    {
        await using var context = CreateInMemoryDbContext();
        var wallet = new WalletEntity { Id = Guid.NewGuid(), Balance = 100, CurrencyCodeId = 1 };
        context.Wallets.Add(wallet);
        await context.SaveChangesAsync();

        var controller = new WalletController(context, _currencyServiceMock.Object, _strategyFactoryMock.Object);

        _currencyServiceMock.Setup(s => s.GetCurrencyIdByCodeAsync("XXX")).ReturnsAsync((int?)null);
        _currencyServiceMock.Setup(s => s.GetCurrencyCodesAsync()).ReturnsAsync(new List<string> { "USD", "EUR" });

        var result = await controller.AdjustBalance(wallet.Id, 10, "XXX", "add");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Currency code not found", badRequest.Value.ToString());
    }

    [Fact]
    public async Task AdjustBalance_AddFundsStrategy_Success()
    {
        await using var context = CreateInMemoryDbContext();
        var wallet = new WalletEntity { Id = Guid.NewGuid(), Balance = 100, CurrencyCodeId = 1 };
        context.Wallets.Add(wallet);
        await context.SaveChangesAsync();

        var strategyMock = new Mock<IBalanceStrategy>();
        strategyMock.Setup(s => s.AdjustBalance(wallet, 50)).Returns(150);

        _strategyFactoryMock.Setup(f => f.GetStrategy("add")).Returns(strategyMock.Object);
        _currencyServiceMock.Setup(s => s.GetCurrencyIdByCodeAsync("USD")).ReturnsAsync(1);

        var controller = new WalletController(context, _currencyServiceMock.Object, _strategyFactoryMock.Object);

        var result = await controller.AdjustBalance(wallet.Id, 50, "USD", "add");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);

        var walletResponse = Assert.IsType<AdjustBalanceResponse>(ok.Value!);

        Assert.Equal(150, walletResponse.Balance);
        strategyMock.Verify(s => s.AdjustBalance(wallet, 50), Times.Once);
    }
}