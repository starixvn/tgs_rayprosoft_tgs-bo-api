using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IBrandGameProviderScheduleService
{
    Task<BrandGameProviderSchedule> GetBrandGameProviderScheduleAsync(
        Expression<Func<BrandGameProviderSchedule, bool>> predicate,
        Func<IQueryable<BrandGameProviderSchedule>, IQueryable<BrandGameProviderSchedule>> preQuery =
            null);

    IQueryable<BrandGameProviderSchedule> GetBrandGameProviderScheduleQueries(
        Func<IQueryable<BrandGameProviderSchedule>, IQueryable<BrandGameProviderSchedule>> preQuery = null,
        bool tracking = false);

    Task UpdateBrandGameProviderDefaultTimesAsync(BrandGameProviderSchedule[] schedules,
        UpdateDefaultTimesRequest request);

    Task UpdateBrandGameProviderScheduleAsync(BrandGameProviderSchedule schedule);
}

public class BrandGameProviderScheduleService : BaseRepositoryOperations<ITgsRepository>,
    IBrandGameProviderScheduleService
{
    private readonly ITgsRepository _repository;

    public BrandGameProviderScheduleService(ITgsRepository tgsRepository) : base(tgsRepository)
    {
        _repository = tgsRepository;
    }

    Task<BrandGameProviderSchedule> IBrandGameProviderScheduleService.GetBrandGameProviderScheduleAsync(
        Expression<Func<BrandGameProviderSchedule, bool>> predicate,
        Func<IQueryable<BrandGameProviderSchedule>, IQueryable<BrandGameProviderSchedule>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    IQueryable<BrandGameProviderSchedule> IBrandGameProviderScheduleService.GetBrandGameProviderScheduleQueries(
        Func<IQueryable<BrandGameProviderSchedule>, IQueryable<BrandGameProviderSchedule>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    async Task IBrandGameProviderScheduleService.UpdateBrandGameProviderDefaultTimesAsync(
        BrandGameProviderSchedule[] schedules, UpdateDefaultTimesRequest request)
    {
        await using var tran = await _repository.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await DeleteEntitiesAsync(schedules);

        var addSchedules = request.DefaultTimes.Select(x => new BrandGameProviderSchedule
        {
            Id = Guid.NewGuid(),
            BrandGameProviderId = request.BrandGameProviderId,
            From = x.From,
            To = x.To,
            Status = request.ScheduleStatus,
            IsDefault = false
        }).ToArray();

        await CreateEntitiesAsync(addSchedules);
        await tran.CommitAsync();
    }

    Task IBrandGameProviderScheduleService.UpdateBrandGameProviderScheduleAsync(
            BrandGameProviderSchedule schedule) =>
        UpdateEntityAsync(schedule);
}
