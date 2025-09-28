using CurrencyUpdater.Data.EntityModels;

namespace WalletApi.Services.Wallet;

public interface IBalanceStrategy
{
    decimal AdjustBalance(WalletEntity wallet, decimal amount);
}