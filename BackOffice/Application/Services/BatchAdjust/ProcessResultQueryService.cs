using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Services.BatchAdjust;

internal class ProcessResultQueryService : IQueryService<ProcessResultQuery, BatchAdjustBalanceItemResult>
{
    private readonly IBatchTempRepository _batchTempRepository;

    public ProcessResultQueryService(IBatchTempRepository batchTempRepository)
    {
        _batchTempRepository = batchTempRepository ?? throw new ArgumentNullException(nameof(batchTempRepository));
    }

    public IAsyncEnumerable<BatchAdjustBalanceItemResult> QueryAsync(ProcessResultQuery query, CancellationToken cancellationToken = default)
        => _batchTempRepository.GetAdjustBalanceItemResultsAsync(query.BatchId, cancellationToken);
}
