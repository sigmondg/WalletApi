namespace CurrencyUpdater.Data.EntityModels;

public class WalletEntity
{
    public Guid Id { get; set; }
    public decimal Balance { get; set; }
    public int CurrencyCodeId { get; set; }
    public CurrencyCodeEntity CurrencyCode { get; set; }
}