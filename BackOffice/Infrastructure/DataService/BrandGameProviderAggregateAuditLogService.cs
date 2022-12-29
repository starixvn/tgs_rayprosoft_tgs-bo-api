using System;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.Application.Models.Aggregate;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IBrandGameProviderAggregateAuditLogService
{
    ValueTask<int> CreateBrandGameProviderAggregateAuditLogAsync(
        CreateBrandGameProviderAggregateAuditLogRequest request);
}

public class BrandGameProviderAggregateAuditLogService : BaseRepositoryOperations<ITgsRepository>,
    IBrandGameProviderAggregateAuditLogService
{
    public BrandGameProviderAggregateAuditLogService(ITgsRepository repository) : base(repository)
    {
    }

    async ValueTask<int> IBrandGameProviderAggregateAuditLogService.CreateBrandGameProviderAggregateAuditLogAsync(
        CreateBrandGameProviderAggregateAuditLogRequest request)
    {
        var log = new BrandGameProviderAggregateAuditLog
        {
            BrandId = request.BrandId,
            CreatedByUserId = request.UserId,
            CreatedOn = DateTimeOffset.UtcNow
        };

        await CreateEntityAsync(log);

        return log.Id;
    }
}
