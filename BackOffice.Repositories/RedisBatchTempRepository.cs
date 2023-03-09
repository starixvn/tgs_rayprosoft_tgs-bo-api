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

    public async ValueTask CompleteBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var key = $"BackOffice:BatchProcess:{batchId}";

        _ = await _database.StringSetAsync(
            key,
            "Completed",
            expiry: TimeSpan.FromHours(1D),
            when: When.Exists).ConfigureAwait(false);
    }

    public IAsyncEnumerable<BatchAdjustBalanceItem> GetAdjustBalanceBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var key = new RedisKey($"BackOffice:AdjustBalance:Batch:{batchId}");

        return _database.StringGetAsync(key)
            .ToAsyncEnumerable()
            .Select(value => JsonSerializer.Deserialize<BatchAdjustBalanceItem[]>(value!, BackOfficeUtility.SerializerOptions))
            .Where(arr => arr is not null)
            .SelectMany(arr => arr!.ToAsyncEnumerable());
    }

    public async ValueTask<string?> GetAdjustBalanceBatchClientTokenAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var key = new RedisKey($"BackOffice:AdjustBalance:Batch:{batchId}:Client");

        return await _database.StringGetAsync(key).ConfigureAwait(false);
    }

    public IAsyncEnumerable<BatchAdjustBalanceItemResult> GetAdjustBalanceItemResultsAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var key = $"BackOffice:AdjustBalance:BatchResult:{batchId}";

        return _database.StringGetAsync(key)
            .ToAsyncEnumerable()
            .Select(value => JsonSerializer.Deserialize<BatchAdjustBalanceItemResult[]>(value!, BackOfficeUtility.SerializerOptions))
            .Where(arr => arr is not null)
            .SelectMany(arr => arr!.ToAsyncEnumerable());
    }

    public async ValueTask<bool?> GetBatchIsCompletedAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var key = $"BackOffice:BatchProcess:{batchId}";

        var value = await _database.StringGetAsync(key).ConfigureAwait(false);

        return value.HasValue ? value == "Completed" : null;
    }

    public async ValueTask<OfflineTempUserBatch?> GetOfflineTempUserBatchAsync(
        Guid batchId,
        CancellationToken cancellationToken = default)
    {
        var key = $"BackOffice:OfflinePromotion:Batch:{batchId}";

        var value = await _database.StringGetAsync(key).ConfigureAwait(false);

        return value.HasValue
            ? JsonSerializer.Deserialize<OfflineTempUserBatch>(value!, BackOfficeUtility.SerializerOptions)
            : null;
    }

    public async ValueTask<OfflineUsersResult?> GetOfflineUsersResultAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        var key = $"BackOffice:OfflinePromotion:BatchResult:{batchId}";

        var content = await _database.StringGetAsync(key).ConfigureAwait(false);

        var result = JsonSerializer.Deserialize<OfflineUsersResult>(content!, BackOfficeUtility.SerializerOptions);

        return result;
    }

    public async ValueTask<Guid> SaveAdjustBalanceBatchAsync(
        IEnumerable<BatchAdjustBalanceItem> batchAdjustBalanceItems,
        string? clientToken,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();

        var key = new RedisKey($"BackOffice:AdjustBalance:Batch:{id}");

        if (!string.IsNullOrWhiteSpace(clientToken))
            _ = await _database.StringSetAsync(
                key.Append("Client"),
                clientToken,
                expiry: TimeSpan.FromHours(1D),
                when: When.NotExists).ConfigureAwait(false);

        var content = JsonSerializer.Serialize(batchAdjustBalanceItems, BackOfficeUtility.SerializerOptions);

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
        var key = $"BackOffice:AdjustBalance:BatchResult:{batchId}";

        var content = JsonSerializer.Serialize(results, BackOfficeUtility.SerializerOptions);

        _ = await _database.StringSetAsync(
            key,
            content,
            expiry: TimeSpan.FromHours(1D)).ConfigureAwait(false);
    }

    public async ValueTask<Guid> SaveOfflineTempUserBatchAsync(
        OfflineTempUserBatch tempBatch,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();

        var key = $"BackOffice:OfflinePromotion:Batch:{id}";

        var content = JsonSerializer.Serialize(tempBatch, BackOfficeUtility.SerializerOptions);

        _ = await _database.StringSetAsync(
            key,
            content,
            expiry: TimeSpan.FromHours(1D),
            when: When.NotExists).ConfigureAwait(false);

        return id;
    }

    public async ValueTask SaveProcessedOfflinePlayerItemsAsync(Guid batchId, OfflineUsersResult result, CancellationToken cancellationToken = default)
    {
        var key = $"BackOffice:OfflinePromotion:BatchResult:{batchId}";

        var content = JsonSerializer.Serialize(result, BackOfficeUtility.SerializerOptions);

        _ = await _database.StringSetAsync(
            key,
            content,
            expiry: TimeSpan.FromHours(1D),
            when: When.NotExists).ConfigureAwait(false);
    }
}
