using System;
using System.Linq;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IBrandLanguageService
{
    IQueryable<BrandLanguage> GetBrandLanguagesQueries(
        Func<IQueryable<BrandLanguage>, IQueryable<BrandLanguage>> preQuery = null,
        bool tracking = false);
}

public class BrandLanguageService : BaseRepositoryOperations<ITgsRepository>, IBrandLanguageService
{
    public BrandLanguageService(ITgsRepository tgsRepository) : base(tgsRepository)
    {
    }

    IQueryable<BrandLanguage> IBrandLanguageService.GetBrandLanguagesQueries(
        Func<IQueryable<BrandLanguage>, IQueryable<BrandLanguage>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);
}
