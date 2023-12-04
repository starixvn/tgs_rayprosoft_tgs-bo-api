using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.BatchAdjust;

internal class PreviewBatchQueryService(IBatchTempRepository batchTempRepository) : IQueryService<PreviewBatchQuery, BatchAdjustBalanceItem>
{
    private readonly IBatchTempRepository _batchTempRepository = batchTempRepository ?? throw new ArgumentNullException(nameof(batchTempRepository));

    public IAsyncEnumerable<BatchAdjustBalanceItem> QueryAsync(PreviewBatchQuery query, CancellationToken cancellationToken = default)
        => _batchTempRepository.GetAdjustBalanceBatchAsync(query.BatchId, cancellationToken);
}
