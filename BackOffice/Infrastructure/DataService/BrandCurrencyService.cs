using System;
using System.Linq;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IBrandCurrencyService
{
    IQueryable<BrandCurrency> GetBrandCurrenciesQueries(
        Func<IQueryable<BrandCurrency>, IQueryable<BrandCurrency>> preQuery = null,
        bool tracking = false);
}

public class BrandCurrencyService : BaseRepositoryOperations<ITgsRepository>, IBrandCurrencyService
{
    public BrandCurrencyService(ITgsRepository tgsRepository) : base(tgsRepository)
    {
    }

    IQueryable<BrandCurrency> IBrandCurrencyService.GetBrandCurrenciesQueries(
        Func<IQueryable<BrandCurrency>, IQueryable<BrandCurrency>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);
}