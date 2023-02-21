using System;
using System.Collections.Generic;
using System.Threading;
using TGS.Services.BackOffice.Domain;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.OfflineLoginReward;

internal class OfflineLoginRewardPlayerItemsQueryService
    : IQueryService<OfflineLoginRewardPlayerItemsQuery, OfflineLoginRewardPlayerItem>
{
    private readonly IBatchTempRepository _batchTempRepository;

    public OfflineLoginRewardPlayerItemsQueryService(
        IBatchTempRepository batchTempRepository)
    {
        _batchTempRepository = batchTempRepository ?? throw new ArgumentNullException(nameof(batchTempRepository));
    }

    public IAsyncEnumerable<OfflineLoginRewardPlayerItem> QueryAsync(
        OfflineLoginRewardPlayerItemsQuery query,
        CancellationToken cancellationToken = default) 
        => _batchTempRepository.GetOfflineLoginRewardPlayerItemsAsync(query.BatchId, cancellationToken);
}