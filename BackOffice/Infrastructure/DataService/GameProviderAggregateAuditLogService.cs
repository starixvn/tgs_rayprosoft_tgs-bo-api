using System;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.Application.Models.Aggregate;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IGameProviderAggregateAuditLogService
{
    ValueTask<int> CreateGameProviderAggregateAuditLogAsync(
        CreateGameProviderAggregateAuditLogRequest request);
}

public class GameProviderAggregateAuditLogService(ITgsRepository repository) : BaseRepositoryOperations<ITgsRepository>(repository),
    IGameProviderAggregateAuditLogService
{
    async ValueTask<int> IGameProviderAggregateAuditLogService.CreateGameProviderAggregateAuditLogAsync(
        CreateGameProviderAggregateAuditLogRequest request)
    {
        var log = new GameProviderAggregateAuditLog
        {
            GameProviderId = request.GameProviderId,
            CreatedByUserId = request.UserId,
            CreatedOn = DateTimeOffset.UtcNow
        };

        await CreateEntityAsync(log);

        return log.Id;
    }
}
