using WalletApi.Services.Wallet.Strategies;

namespace WalletApi.Services.Wallet;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWalletService(this IServiceCollection services)
    {
        services.AddScoped<IBalanceStrategy, AddFundsStrategy>();
        services.AddScoped<IBalanceStrategy, SubtractFundsStrategy>();
        services.AddScoped<IBalanceStrategy, ForceSubtractFundsStrategy>();
        services.AddSingleton<IBalanceStrategyFactory, BalanceStrategyFactory>();

        return services;
    }
}