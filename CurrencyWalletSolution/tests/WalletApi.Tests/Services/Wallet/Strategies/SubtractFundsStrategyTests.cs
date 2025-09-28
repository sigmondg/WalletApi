using CurrencyUpdater.Data.EntityModels;
using WalletApi.Services.Wallet.Strategies;

namespace WalletApi.Tests.Services.Wallet.Strategies;

public class SubtractFundsStrategyTests
{
    [Fact]
    public void Adjust_Balance_SubtractAmountFromWallet()
    {
        var wallet = new WalletEntity { Balance = 100 };
        var strategy = new SubtractFundsStrategy();

        wallet.Balance = strategy.AdjustBalance(wallet, 50);

        Assert.Equal(50, wallet.Balance);
    }
    
    [Fact]
    public void Adjust_Balance_SubtractNegativeAmountFromWallet()
    {
        var wallet = new WalletEntity { Balance = 100 };
        var strategy = new SubtractFundsStrategy();

        Assert.Throws<ArgumentException>(() =>
            strategy.AdjustBalance(wallet, -50));
    }
    
    [Fact]
    public void Adjust_Balance_InsufficientFunds()
    {
        var wallet = new WalletEntity { Balance = 100 };
        var strategy = new SubtractFundsStrategy();
        
        var ex =  Assert.Throws<InvalidOperationException>(
            () => strategy.AdjustBalance(wallet, 150));
        
        Assert.Equal("Insufficient funds in wallet", ex.Message);
    }
}