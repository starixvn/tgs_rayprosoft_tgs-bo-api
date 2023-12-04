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

public class BrandCurrencyService(ITgsRepository tgsRepository) : BaseRepositoryOperations<ITgsRepository>(tgsRepository), IBrandCurrencyService
{
    IQueryable<BrandCurrency> IBrandCurrencyService.GetBrandCurrenciesQueries(
        Func<IQueryable<BrandCurrency>, IQueryable<BrandCurrency>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);
}
