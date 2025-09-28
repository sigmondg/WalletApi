using CurrencyUpdater.Data.EntityModels;

namespace WalletApi.Services.Wallet.Strategies;

public class SubtractFundsStrategy : BalanceStrategyBase
{
    protected override decimal Apply(WalletEntity wallet, decimal amount)
    {
        if (wallet.Balance < amount)
            throw new InvalidOperationException("Insufficient funds in wallet");

        return wallet.Balance - amount;
    }
}