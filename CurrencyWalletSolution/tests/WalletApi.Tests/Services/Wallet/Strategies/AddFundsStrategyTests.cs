using CurrencyUpdater.Data.EntityModels;
using WalletApi.Services.Wallet.Strategies;

namespace WalletApi.Tests.Services.Wallet.Strategies;

public class AddFundsStrategyTests
{
    [Fact]
    public void Adjust_Balance_AddsAmountToWallet()
    {
        var wallet = new WalletEntity { Balance = 100 };
        var strategy = new AddFundsStrategy();

        wallet.Balance = strategy.AdjustBalance(wallet, 50);

        Assert.Equal(150, wallet.Balance);
    }

    [Fact]
    public void Adjust_Balance_NegativeAmount()
    {
        var wallet = new WalletEntity { Balance = 100 };
        var strategy = new AddFundsStrategy();

        Assert.Throws<ArgumentException>(() =>
            strategy.AdjustBalance(wallet, -50));
    }
}