using WalletApi.Services.Wallet.Strategies;

namespace WalletApi.Services.Wallet;

public interface IBalanceStrategyFactory
{
    IBalanceStrategy GetStrategy(string strategyName);
}

public class BalanceStrategyFactory : IBalanceStrategyFactory
{
    private readonly Dictionary<string, IBalanceStrategy> _strategies;

    public BalanceStrategyFactory()
    {
        _strategies = new Dictionary<string, IBalanceStrategy>(StringComparer.OrdinalIgnoreCase)
        { { "add", new AddFundsStrategy() },
          { "subtract", new SubtractFundsStrategy() },
          { "forcesubtract", new ForceSubtractFundsStrategy() } };
    }

    public virtual IBalanceStrategy GetStrategy(string strategyName)
    {
        if (!_strategies.TryGetValue(strategyName, out IBalanceStrategy? value))
            throw new ArgumentException($"Invalid strategy name: {strategyName}, Allowed: {string.Join(",", _strategies.Keys)}");

        return value;
    }
}