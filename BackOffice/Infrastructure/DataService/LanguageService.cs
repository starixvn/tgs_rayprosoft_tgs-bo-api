using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.Application.Models.Language;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface ILanguageService
{
    Task CreateLanguageAsync(CreateLanguageRequest request);

    Task<bool> ExistsAsync(Expression<Func<Language, bool>> predicate);

    Task<Language> GetLanguageAsync(Expression<Func<Language, bool>> predicate,
                Func<IQueryable<Language>, IQueryable<Language>> preQuery = null);

    IQueryable<Language> GetLanguageQueries(
        Func<IQueryable<Language>, IQueryable<Language>> preQuery = null,
        bool tracking = false);

    Task UpdateLanguageAsync(Language language, UpdateLanguageRequest request);
}

public class LanguageService(ITgsRepository tgsRepository) : BaseRepositoryOperations<ITgsRepository>(tgsRepository), ILanguageService
{
    async Task ILanguageService.CreateLanguageAsync(CreateLanguageRequest request)
    {
        var language = new Language
        {
            Name = request.Name ?? string.Empty,
            CultureCode = request.CultureCode,
            LanguageCode = request.LanguageCode,
            LocalizedName = request.LocalizedName ?? string.Empty,
            CreatedByUsername = request.EditorUsername,
            UpdatedByUsername = request.EditorUsername,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now
        };

        await CreateEntityAsync(language);
    }

    Task<bool> ILanguageService.ExistsAsync(Expression<Func<Language, bool>> predicate) =>
        ExistsAsync(predicate);

    Task<Language> ILanguageService.GetLanguageAsync(Expression<Func<Language, bool>> predicate,
                Func<IQueryable<Language>, IQueryable<Language>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    IQueryable<Language> ILanguageService.GetLanguageQueries(
        Func<IQueryable<Language>, IQueryable<Language>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    async Task ILanguageService.UpdateLanguageAsync(
        Language language, UpdateLanguageRequest request)
    {
        language.Name = request.Name;
        language.CultureCode = request.CultureCode;
        language.LanguageCode = request.LanguageCode;
        language.LocalizedName = request.LocalizedName;
        language.UpdatedOn = DateTimeOffset.Now;

        await UpdateEntityAsync(language);
    }
}
