namespace CurrencyUpdater.Data.EntityModels;

public class CurrencyRateEntity
{
    public Guid Id { get; set; }
    public int CurrencyCodeId { get; set; }
    public DateTime RateDate { get; set; }
    public decimal Rate { get; set; }
}