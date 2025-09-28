namespace WalletApi.Models;

public class CreateWalletRequest
{
    public decimal Balance { get; set; }
    public string CurrencyCode { get; set; }
}

public abstract class BaseWalletResponse
{
    public Guid Id { get; set; }
}

public class CreateWalletResponse : BaseWalletResponse
{
}

public class AdjustBalanceResponse : BaseWalletResponse
{
    public decimal Balance { get; set; }
    public string CurrencyCode { get; set; }
}

public class GetBalanceResponse : BaseWalletResponse
{
    public decimal Balance { get; set; }
    public string CurrencyCode { get; set; }
}