using System;
using System.Collections.Generic;
using System.Threading;
using TGS.Services.BackOffice.Domain;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;

internal class GetProcessedUsersQueryService
    : IQueryService<GetProcessedUsersQuery, OfflinePlayerItem>
{
    private readonly IBatchTempRepository _batchTempRepository;

    public GetProcessedUsersQueryService(
        IBatchTempRepository batchTempRepository)
    {
        _batchTempRepository = batchTempRepository ?? throw new ArgumentNullException(nameof(batchTempRepository));
    }

    public IAsyncEnumerable<OfflinePlayerItem> QueryAsync(
        GetProcessedUsersQuery query,
        CancellationToken cancellationToken = default)
        => _batchTempRepository.GetOfflineLoginRewardPlayerItemsAsync(query.BatchId, cancellationToken);
}