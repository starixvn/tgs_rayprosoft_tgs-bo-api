#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
using TGS.Services.BackOffice.Domain;

namespace TGS.BackOffice.Repositories;

internal class RedisBatchTempRepository : IBatchTempRepository
{
    private readonly IDatabase _database;

    public RedisBatchTempRepository(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public async ValueTask<bool> BidBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var key = $"BackOffice:BatchProcess:{batchId}";

        var machineName = Environment.MachineName;

        return await _database.StringSetAsync(
            key,
            machineName,
            expiry: TimeSpan.FromHours(1D),
            when: When.NotExists).ConfigureAwait(false);
    }

    public async ValueTask<bool> BidOfflineLoginRewardBackgroundProcessAsync(
        Guid batchId, 
        CancellationToken cancellationToken = default)
    {
        var key = $"BackOffice:OfflineLoginReward:BackgroundProcess:{batchId}";

        var machineName = Environment.MachineName;

        return await _database.StringSetAsync(
            key,
            machineName,
            expiry: TimeSpan.FromHours(1D),
            when: When.NotExists).ConfigureAwait(false);
    }

    public async ValueTask CompleteBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var key = $"BackOffice:BatchProcess:{batchId}";

        _ = await _database.StringSetAsync(
            key,
            "Completed",
            expiry: TimeSpan.FromHours(1D),
            when: When.Exists).ConfigureAwait(false);
    }

    public async ValueTask CompleteOfflineLoginRewardBackgroundProcessAsync(
        Guid batchId, 
        CancellationToken cancellationToken = default)
    {
        var key = $"BackOffice:OfflineLoginReward:BackgroundProcess:{batchId}";

        _ = await _database.StringSetAsync(
            key,
            "Completed",
            expiry: TimeSpan.FromHours(1D),
            when: When.Exists).ConfigureAwait(false);
    }

    public IAsyncEnumerable<BatchAdjustBalanceItem> GetAdjustBalanceBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var key = $"BackOffice:Batch:{batchId}";

        return _database.StringGetAsync(key)
            .ToAsyncEnumerable()
            .Select(value => JsonSerializer.Deserialize<BatchAdjustBalanceItem[]>(value!))
            .Where(arr => arr is not null)
            .SelectMany(arr => arr!.ToAsyncEnumerable());
    }

    public IAsyncEnumerable<BatchAdjustBalanceItemResult> GetAdjustBalanceItemResultsAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var key = $"BackOffice:BatchResult:{batchId}";

        return _database.StringGetAsync(key)
            .ToAsyncEnumerable()
            .Select(value => JsonSerializer.Deserialize<BatchAdjustBalanceItemResult[]>(value!))
            .Where(arr => arr is not null)
            .SelectMany(arr => arr!.ToAsyncEnumerable());
    }

    public async ValueTask<bool?> GetBatchIsCompletedAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var key = $"BackOffice:BatchProcess:{batchId}";

        var value = await _database.StringGetAsync(key).ConfigureAwait(false);

        return value.HasValue ? value == "Completed" : null;
    }

    public async ValueTask<bool?> GetOfflineLoginRewardBackgroundProcessIsCompletedAsync(Guid batchId,
        CancellationToken cancellationToken = default)
    {
        var key = $"BackOffice:OfflineLoginReward:BackgroundProcess:{batchId}";

        var value = await _database.StringGetAsync(key).ConfigureAwait(false);

        return value.HasValue ? value == "Completed" : null;
    }

    public IAsyncEnumerable<OfflineLoginRewardPlayerItem> GetOfflineLoginRewardPlayerItemsAsync(
        Guid batchId,
        CancellationToken cancellationToken = default)
    {
        var key = $"BackOffice:OfflineLoginReward:PlayerItems:{batchId}";

        return _database.StringGetAsync(key)
            .ToAsyncEnumerable()
            .Select(value => JsonSerializer.Deserialize<OfflineLoginRewardPlayerItem[]>(value!))
            .Where(arr => arr is not null)
            .SelectMany(arr => arr!.ToAsyncEnumerable());
    }

    public async ValueTask<OfflineLoginRewardTempUserBatch?> GetOfflineLoginRewardTempUserBatchAsync(
        Guid batchId,
        CancellationToken cancellationToken = default)
    {
        var key = $"BackOffice:OfflineLoginReward:TempUserBatch:{batchId}";

        var value = await _database.StringGetAsync(key).ConfigureAwait(false);

        return value.HasValue
            ? JsonSerializer.Deserialize<OfflineLoginRewardTempUserBatch>(value!)
            : null;
    }
    public async ValueTask<Guid> SaveAdjustBalanceBatchAsync(
        IEnumerable<BatchAdjustBalanceItem> batchAdjustBalanceItems,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();

        var key = $"BackOffice:Batch:{id}";

        var content = JsonSerializer.Serialize(batchAdjustBalanceItems);

        _ = await _database.StringSetAsync(
            key,
            content,
            expiry: TimeSpan.FromHours(1D),
            when: When.NotExists).ConfigureAwait(false);

        return id;
    }

    public async ValueTask SaveAdjustBalanceBatchResultAsync(
        Guid batchId,
        IEnumerable<BatchAdjustBalanceItemResult> results,
        CancellationToken cancellation = default)
    {
        var key = $"BackOffice:BatchResult:{batchId}";

        var content = JsonSerializer.Serialize(results);

        _ = await _database.StringSetAsync(
            key,
            content,
            expiry: TimeSpan.FromHours(1D)).ConfigureAwait(false);
    }

    public async ValueTask SaveOfflineLoginRewardPlayerItemsAsync(
        Guid batchId,
        IEnumerable<OfflineLoginRewardPlayerItem> playerItems,
        CancellationToken cancellationToken = default)
    {
        var key = $"BackOffice:OfflineLoginReward:PlayerItems:{batchId}";

        var content = JsonSerializer.Serialize(playerItems);

        _ = await _database.StringSetAsync(
            key,
            content,
            expiry: TimeSpan.FromHours(1D),
            when: When.NotExists).ConfigureAwait(false);
    }

    public async ValueTask<Guid> SaveOfflineLoginRewardTempUserBatchAsync(
        OfflineLoginRewardTempUserBatch tempBatch,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();

        var key = $"BackOffice:OfflineLoginReward:TempUserBatch:{id}";

        var content = JsonSerializer.Serialize(tempBatch);

        _ = await _database.StringSetAsync(
            key,
            content,
            expiry: TimeSpan.FromHours(1D),
            when: When.NotExists).ConfigureAwait(false);

        return id;
    }
}
