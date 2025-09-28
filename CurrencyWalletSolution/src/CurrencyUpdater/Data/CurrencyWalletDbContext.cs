using CurrencyUpdater.Data.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace CurrencyUpdater.Data;

public class CurrencyWalletDbContext : DbContext
{
    public CurrencyWalletDbContext()
    {
    }

    public CurrencyWalletDbContext(DbContextOptions<CurrencyWalletDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("dbo");

        builder.Entity<CurrencyRateEntity>(entity =>
        {
            entity.Property(e => e.Rate)
                .HasPrecision(18, 5);
        });

        builder.Entity<WalletEntity>(entity =>
        {
            entity.Property(e => e.Balance)
                .HasPrecision(18, 2);
        });

        builder.Entity<CurrencyRateEntity>()
            .ToTable("CurrencyRates")
            .Property(p => p.Id)
            .HasDefaultValueSql("NEWID()")
            .ValueGeneratedOnAdd();

        builder.Entity<WalletEntity>()
            .ToTable("Wallets")
            .Property(p => p.Id)
            .HasDefaultValueSql("NEWID()")
            .ValueGeneratedOnAdd();
    }

    public DbSet<CurrencyRateEntity> CurrencyRates { get; set; }
    public DbSet<WalletEntity> Wallets { get; set; }
    public DbSet<CurrencyCodeEntity> CurrencyCodes { get; set; }
}