using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;
using TGS.Services.BackOffice.Application.Models.SubGameProvider;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface ISubGameProviderService
{
    Task<SubGameProvider> GetSubGameProviderAsync(
        Expression<Func<SubGameProvider, bool>> predicate,
        Func<IQueryable<SubGameProvider>, IQueryable<SubGameProvider>> preQuery =
            null);

    IQueryable<SubGameProvider> GetSubGameProviderQueries(
        Func<IQueryable<SubGameProvider>, IQueryable<SubGameProvider>> preQuery = null,
        bool tracking = false);

    Task CreateSubGameProviderAsync(CreateSubGameProviderRequest request);
    Task UpdateSubGameProviderAsync(SubGameProvider gameProvider, UpdateSubGameProviderRequest request);
    Task<bool> ExistsAsync(Expression<Func<SubGameProvider, bool>> predicate);
}

public class SubGameProviderService : BaseRepositoryOperations<ITgsRepository>, ISubGameProviderService
{
    private readonly ITgsRepository _repository;

    public SubGameProviderService(ITgsRepository tgsRepository) : base(
        tgsRepository)
    {
        _repository = tgsRepository;
    }

    Task<SubGameProvider> ISubGameProviderService.GetSubGameProviderAsync(
        Expression<Func<SubGameProvider, bool>> predicate,
        Func<IQueryable<SubGameProvider>, IQueryable<SubGameProvider>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    IQueryable<SubGameProvider> ISubGameProviderService.GetSubGameProviderQueries(
        Func<IQueryable<SubGameProvider>, IQueryable<SubGameProvider>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    async Task ISubGameProviderService.UpdateSubGameProviderAsync(
        SubGameProvider gameProvider, UpdateSubGameProviderRequest request)
    {
        gameProvider.Name = request.Name;
        gameProvider.Description = request.Description;
        gameProvider.Localization = request.Localization?.ToJson();
        gameProvider.UpdatedByUsername = request.EditorUsername;
        gameProvider.UpdatedOn = DateTimeOffset.Now;

        await UpdateEntityAsync(gameProvider);
    }

    async Task ISubGameProviderService.CreateSubGameProviderAsync(CreateSubGameProviderRequest request)
    {
        var gameProvider = new SubGameProvider
        {
            GameProviderId = request.GameProviderId,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Localization = request.Localization?.ToJson(),
            CreatedByUsername = request.EditorUsername,
            UpdatedByUsername = request.EditorUsername,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now
        };

        await CreateEntityAsync(gameProvider);
    }

    Task<bool> ISubGameProviderService.ExistsAsync(Expression<Func<SubGameProvider, bool>> predicate) =>
        ExistsAsync(predicate);
}