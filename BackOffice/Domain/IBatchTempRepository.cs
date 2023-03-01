#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TGS.Services.BackOffice.Domain;

public interface IBatchTempRepository
{
    ValueTask<bool> BidBatchAsync(Guid batchId, CancellationToken cancellationToken = default);

    ValueTask CompleteBatchAsync(Guid batchId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<BatchAdjustBalanceItem> GetAdjustBalanceBatchAsync(
        Guid batchId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<BatchAdjustBalanceItemResult> GetAdjustBalanceItemResultsAsync(
        Guid batchId,
        CancellationToken cancellationToken = default);

    ValueTask<bool?> GetBatchIsCompletedAsync(Guid batchId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<OfflinePlayerItem> GetOfflineLoginRewardPlayerItemsAsync(
        Guid batchId,
        CancellationToken cancellationToken = default);

    ValueTask<OfflineTempUserBatch?> GetOfflineTempUserBatchAsync(
        Guid batchId,
        CancellationToken cancellationToken = default);

    ValueTask<Guid> SaveAdjustBalanceBatchAsync(
        IEnumerable<BatchAdjustBalanceItem> batchAdjustBalanceItems,
        CancellationToken cancellationToken = default);

    ValueTask SaveAdjustBalanceBatchResultAsync(
        Guid batchId,
        IEnumerable<BatchAdjustBalanceItemResult> results,
        CancellationToken cancellation = default);

    ValueTask<Guid> SaveOfflineTempUserBatchAsync(
        OfflineTempUserBatch tempBatch,
        CancellationToken cancellationToken = default);

    ValueTask SaveProcessedOfflinePlayerItemsAsync(
            Guid batchId,
        OfflineUsersResult result,
        CancellationToken cancellationToken = default);
}
