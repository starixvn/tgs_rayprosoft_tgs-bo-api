using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.Application.Models.Currency;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface ICurrencyService
{
    Task CreateCurrencyAsync(CreateCurrencyRequest request);

    Task<bool> ExistsAsync(Expression<Func<Currency, bool>> predicate);

    IQueryable<Currency> GetCurrencieQueries(
        Func<IQueryable<Currency>, IQueryable<Currency>> preQuery = null,
        bool tracking = false);

    Task<Currency> GetCurrencyAsync(Expression<Func<Currency, bool>> predicate,
                    Func<IQueryable<Currency>, IQueryable<Currency>> preQuery = null);

    Task UpdateCurrencyAsync(Currency currency, UpdateCurrencyRequest request);
}

public class CurrencyService : BaseRepositoryOperations<ITgsRepository>, ICurrencyService
{
    public CurrencyService(ITgsRepository tgsRepository) : base(tgsRepository)
    {
    }

    async Task ICurrencyService.CreateCurrencyAsync(CreateCurrencyRequest request)
    {
        var currency = new Currency
        {
            Name = request.Name,
            Code = request.Code,
            NativeName = request.NativeName,
            Symbol = request.Symbol,
            CreatedByUsername = request.EditorUsername,
            UpdatedByUsername = request.EditorUsername,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now
        };

        await CreateEntityAsync(currency);
    }

    Task<bool> ICurrencyService.ExistsAsync(Expression<Func<Currency, bool>> predicate) =>
        ExistsAsync(predicate);

    IQueryable<Currency> ICurrencyService.GetCurrencieQueries(
        Func<IQueryable<Currency>, IQueryable<Currency>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    Task<Currency> ICurrencyService.GetCurrencyAsync(Expression<Func<Currency, bool>> predicate,
                    Func<IQueryable<Currency>, IQueryable<Currency>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    async Task ICurrencyService.UpdateCurrencyAsync(
        Currency currency, UpdateCurrencyRequest request)
    {
        currency.Name = request.Name;
        currency.NativeName = request.NativeName;
        currency.Symbol = request.Symbol;
        currency.UpdatedByUsername = request.EditorUsername;
        currency.UpdatedOn = DateTimeOffset.Now;

        await UpdateEntityAsync(currency);
    }
}
