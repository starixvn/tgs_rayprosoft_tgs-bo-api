using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.BatchAdjust;

internal class CheckBatchCompletedQueryService(IBatchTempRepository batchTempRepository) : IGetService<CheckBatchCompletedQuery, bool?>
{
    private readonly IBatchTempRepository _batchTempRepository = batchTempRepository ?? throw new ArgumentNullException(nameof(batchTempRepository));

    public ValueTask<bool?> GetAsync(CheckBatchCompletedQuery query, CancellationToken cancellationToken = default)
        => _batchTempRepository.GetBatchIsCompletedAsync(query.BatchId, cancellationToken);
}
