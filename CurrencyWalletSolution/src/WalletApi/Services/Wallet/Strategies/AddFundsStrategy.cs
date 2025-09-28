using CurrencyUpdater.Data.EntityModels;

namespace WalletApi.Services.Wallet.Strategies;

public class AddFundsStrategy : BalanceStrategyBase 
{
    protected override decimal Apply(WalletEntity wallet, decimal amount) => wallet.Balance + amount;
}