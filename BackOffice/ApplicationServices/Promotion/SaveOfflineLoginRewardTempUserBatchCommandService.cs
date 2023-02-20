using System;
using System.Threading;
using System.Threading.Tasks;
using TGS.Services.BackOffice.Domain;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal class SaveOfflineLoginRewardTempUserBatchCommandService : IGetService<SaveOfflineLoginRewardTempUserBatchCommand, Guid>
{
    private readonly IBatchTempRepository _batchTempRepository;

    public SaveOfflineLoginRewardTempUserBatchCommandService(
        IBatchTempRepository batchTempRepository)
    {
        _batchTempRepository = batchTempRepository ?? throw new ArgumentNullException(nameof(batchTempRepository));
    }

    public ValueTask<Guid> GetAsync(
        SaveOfflineLoginRewardTempUserBatchCommand command,
        CancellationToken cancellationToken = default)
    {
        var tempBatch = new OfflineLoginRewardTempUserBatch(
            command.BrandId,
            command.UserNames);

        return _batchTempRepository.SaveOfflineLoginRewardTempUserBatchAsync(tempBatch, cancellationToken);
    }
}