using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;

internal class SaveOfflineTempUserBatchCommandService(
    IBatchTempRepository batchTempRepository) : IGetService<SaveOfflineTempUserBatchCommand, Guid>
{
    private readonly IBatchTempRepository _batchTempRepository = batchTempRepository ?? throw new ArgumentNullException(nameof(batchTempRepository));

    public ValueTask<Guid> GetAsync(
        SaveOfflineTempUserBatchCommand command,
        CancellationToken cancellationToken = default)
    {
        var tempBatch = new OfflineTempUserBatch(
            command.BrandId,
            command.ClientToken,
            command.UserNames,
            command.ModifyUserId);

        return _batchTempRepository.SaveOfflineTempUserBatchAsync(tempBatch, cancellationToken);
    }
}
