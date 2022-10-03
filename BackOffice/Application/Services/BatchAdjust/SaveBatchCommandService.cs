using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Services.BatchAdjust;

internal class SaveBatchCommandService : IGetService<SaveBatchCommand, Guid>
{
    private readonly IBatchTempRepository _batchTempRepository;

    public SaveBatchCommandService(IBatchTempRepository batchTempRepository)
    {
        _batchTempRepository = batchTempRepository ?? throw new ArgumentNullException(nameof(batchTempRepository));
    }

    public ValueTask<Guid> GetAsync(SaveBatchCommand query, CancellationToken cancellationToken = default)
        => _batchTempRepository.SaveAdjustBalanceBatchAsync(query.Batch.ToEnumerable(), cancellationToken);
}
