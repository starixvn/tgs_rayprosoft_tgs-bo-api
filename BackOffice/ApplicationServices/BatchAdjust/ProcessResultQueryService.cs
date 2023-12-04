using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.BatchAdjust;

internal class ProcessResultQueryService(IBatchTempRepository batchTempRepository) : IQueryService<ProcessResultQuery, BatchAdjustBalanceItemResult>
{
    private readonly IBatchTempRepository _batchTempRepository = batchTempRepository ?? throw new ArgumentNullException(nameof(batchTempRepository));

    public IAsyncEnumerable<BatchAdjustBalanceItemResult> QueryAsync(ProcessResultQuery query, CancellationToken cancellationToken = default)
        => _batchTempRepository.GetAdjustBalanceItemResultsAsync(query.BatchId, cancellationToken);
}
