using CurrencyUpdater.Data.EntityModels;
using WalletApi.Services.Wallet.Strategies;

namespace WalletApi.Tests.Services.Wallet.Strategies;

public class ForceSubtractFundsStrategyTests
{
    [Fact]
    public void Adjust_Balance_ForceSubtractAmountFromWallet()
    {
        var wallet = new WalletEntity { Balance = 100 };
        var strategy = new ForceSubtractFundsStrategy();

        wallet.Balance = strategy.AdjustBalance(wallet, 150);

        Assert.Equal(-50, wallet.Balance);
    }


    [Fact]
    public void Adjust_Balance_ForceSubtractNegativeAmountFromWallet()
    {
        var wallet = new WalletEntity { Balance = 100 };
        var strategy = new SubtractFundsStrategy();

        Assert.Throws<ArgumentException>(() =>
            strategy.AdjustBalance(wallet, -50));
    }
}