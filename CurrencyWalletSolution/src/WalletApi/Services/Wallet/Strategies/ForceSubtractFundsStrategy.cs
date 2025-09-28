using CurrencyUpdater.Data.EntityModels;

namespace WalletApi.Services.Wallet.Strategies;

public class ForceSubtractFundsStrategy : BalanceStrategyBase
{ 
    protected override decimal Apply(WalletEntity wallet, decimal amount) => wallet.Balance - amount;
}