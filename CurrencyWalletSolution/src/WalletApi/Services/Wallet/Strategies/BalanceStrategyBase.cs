using CurrencyUpdater.Data.EntityModels;

namespace WalletApi.Services.Wallet.Strategies;

public abstract class BalanceStrategyBase : IBalanceStrategy
{
    public decimal AdjustBalance(WalletEntity currentBalance, decimal amount)
    {
        if (amount <= 0)
            throw new  ArgumentException("Amount must be greater than zero");
        
        return Apply(currentBalance, amount);
    }
    
    protected abstract decimal Apply(WalletEntity currentBalance, decimal amount);
}