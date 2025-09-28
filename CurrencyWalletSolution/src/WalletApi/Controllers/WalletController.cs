using System.Collections.Concurrent;
using CurrencyUpdater.Data;
using CurrencyUpdater.Data.EntityModels;
using CurrencyUpdater.Services;
using Microsoft.AspNetCore.Mvc;
using WalletApi.Models;
using WalletApi.Services.Wallet;

namespace WalletApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletController : ControllerBase
{
    private readonly CurrencyWalletDbContext _db;
    private readonly ICurrencyService _currencyService;
    private readonly IBalanceStrategyFactory _balanceStrategyFactory;
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _walletSemaphores = new();


    public WalletController(
        CurrencyWalletDbContext db,
        ICurrencyService currencyService,
        IBalanceStrategyFactory balanceStrategyFactory)
    {
        _db = db;
        _currencyService = currencyService;
        _balanceStrategyFactory = balanceStrategyFactory;
    }

    [HttpPost]
    public async Task<IActionResult> CreateWallet([FromBody] CreateWalletRequest walletModel)
    {
        var currencyId = await _currencyService.GetCurrencyIdByCodeAsync(walletModel.CurrencyCode);

        if (currencyId == null)
        {
            var currencyCodes = await _currencyService.GetCurrencyCodesAsync();
            return BadRequest($"Currency code not found, Please use one of the following: {String.Join(",", currencyCodes)}");
        }

        var wallet = new WalletEntity
        { Id = Guid.NewGuid(),
          Balance = walletModel.Balance,
          CurrencyCodeId = (int)currencyId };

        _db.Wallets.Add(wallet);
        await _db.SaveChangesAsync();
        return Ok(new CreateWalletResponse { Id = wallet.Id });
    }

    [HttpGet("{walletId}")]
    public async Task<IActionResult> GetBalance(Guid walletId, [FromQuery] string? currencyCode)
    {
        var wallet = await _db.Wallets.FindAsync(walletId);

        if (wallet == null) return NotFound($"Wallet with id {walletId} not found");

        var balance = wallet.Balance;
        string responseCurrencyCode;
        if (!string.IsNullOrEmpty(currencyCode))
        {
            var targetCurrencyId = await _currencyService.GetCurrencyIdByCodeAsync(currencyCode);

            if (targetCurrencyId == null)
            {
                var currencyCodes = await _currencyService.GetCurrencyCodesAsync();
                return BadRequest($"Currency code not found, Please use one of the following: {String.Join(",", currencyCodes)}");
            }

            balance = await _currencyService.ConvertAmountAsync(wallet.Balance, wallet.CurrencyCodeId, (int)targetCurrencyId);
            responseCurrencyCode = currencyCode;
        }
        else
            responseCurrencyCode = await _currencyService.GetCurrencyCodeByIdAsync(wallet.CurrencyCodeId);

        return Ok(new GetBalanceResponse
        { Id = walletId,
          Balance = Math.Round(balance, 2),
          CurrencyCode = responseCurrencyCode });
    }

    [HttpPost("{walletId}/adjustbalance")]
    public async Task<IActionResult> AdjustBalance(
        Guid walletId,
        [FromQuery] decimal amount,
        [FromQuery] string currency,
        [FromQuery] string strategy)
    {
        if (amount <= 0)
            return BadRequest("Amount must be greater than zero");

        var semaphore = _walletSemaphores.GetOrAdd(walletId, _ => new SemaphoreSlim(0, 1));

        try
        {
            var wallet = await _db.Wallets.FindAsync(walletId);
            if (wallet == null)
                return NotFound($"Wallet with id {walletId} not found");

            var currencyId = await _currencyService.GetCurrencyIdByCodeAsync(currency);
            if (currencyId == null)
            {
                var currencyCodes = await _currencyService.GetCurrencyCodesAsync();
                return BadRequest($"Currency code not found, Please use one of the following: {String.Join(",", currencyCodes)}");
            }

            var adjustedAmount = amount;

            if (wallet.CurrencyCodeId != currencyId)
                adjustedAmount = await _currencyService.ConvertAmountAsync(amount, (int)currencyId, wallet.CurrencyCodeId);

            try
            {
                var strategyInstance = _balanceStrategyFactory.GetStrategy(strategy);
                wallet.Balance = strategyInstance.AdjustBalance(wallet, adjustedAmount);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            await _db.SaveChangesAsync();

            return Ok(new AdjustBalanceResponse { Id = wallet.Id, Balance = Math.Round(wallet.Balance, 2), CurrencyCode = await _currencyService.GetCurrencyCodeByIdAsync(wallet.CurrencyCodeId) });
        }
        finally
        {
            semaphore.Release();

            if (semaphore.CurrentCount == 1)
            {
                _walletSemaphores.TryRemove(walletId, out _);
                semaphore.Dispose();
            }
        }
    }
}