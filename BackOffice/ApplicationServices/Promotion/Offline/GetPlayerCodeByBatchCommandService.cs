#nullable enable

using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Tgs.BrandService.Protos;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.ApplicationServices.Players;
using Valhalla.MessageQueue;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;

internal class GetPlayerCodeByBatchCommandService(
    IQueryService<PlayerInfosByUserNamesQuery, PlayerInfo> playerInfosQueryService,
    IBatchTempRepository batchTempRepository,
    IMessageSender messageSender) : ICommandService<GetPlayerCodeByBatchCommand>
{
    private static readonly int _SegmentCount = 500;

    private readonly IBatchTempRepository _batchTempRepository = batchTempRepository ?? throw new ArgumentNullException(nameof(batchTempRepository));
    private readonly IMessageSender _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    private readonly IQueryService<PlayerInfosByUserNamesQuery, PlayerInfo> _playerInfosQueryService = playerInfosQueryService ?? throw new ArgumentNullException(nameof(playerInfosQueryService));

    public async ValueTask ExecuteAsync(
        GetPlayerCodeByBatchCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!await _batchTempRepository
            .BidBatchAsync(command.BatchId, cancellationToken)
            .ConfigureAwait(false))
            return;

        var batch = await _batchTempRepository
            .GetOfflineTempUserBatchAsync(command.BatchId, cancellationToken)
            .ConfigureAwait(false);

        if (batch is null)
        {
            await _batchTempRepository
                .CompleteBatchAsync(command.BatchId, cancellationToken)
                .ConfigureAwait(false);

            return;
        }

        var uniqueUsers = batch.UserNames
            .Distinct()
            .ToArray();

        var pages = (int)Math.Ceiling(uniqueUsers.Length / (double)_SegmentCount);

        var userNameMap = (await Enumerable.Range(0, pages)
            .ToAsyncEnumerable()
            .Select(page => _playerInfosQueryService.QueryAsync(
                new PlayerInfosByUserNamesQuery(
                    batch.BrandId,
                    uniqueUsers.Skip(page * _SegmentCount).Take(_SegmentCount)),
                cancellationToken))
            .Merge()
            .ToArrayAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false))
            .ToDictionary(info => info.UserName, info => info.PlayerCode);

        var playerItems = uniqueUsers
            .Select(userName => userNameMap.TryGetValue(userName, out var playerCode)
                ? new OfflinePlayerItem(userName, playerCode)
                : new OfflinePlayerItem(userName, null));

        var result = new OfflineUsersResult(
            playerItems,
            playerItems.Count(item => item.PlayerCode is not null),
            batch.UserNames
                .GroupBy(name => name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key));

        await _batchTempRepository
            .SaveProcessedOfflinePlayerItemsAsync(command.BatchId, result, cancellationToken)
            .ConfigureAwait(false);

        await _batchTempRepository
            .CompleteBatchAsync(command.BatchId, cancellationToken)
            .ConfigureAwait(false);

        var notifyEvent = new NotifyEvent
        {
            Data = JsonSerializer.Serialize(
                new OfflineParsePlayerCodeNotifyBody(command.BatchId, batch.ClientToken),
                BackOfficeUtility.SerializerOptions)
        };

        notifyEvent.Tags.Add(new Tag
        {
            Name = "UserId",
            Value = batch.ModifyUserId.ToString()
        });
        notifyEvent.Tags.Add(new Tag
        {
            Name = "Feature",
            Value = "OfflinePromotion"
        });

        await _messageSender.PublishAsync<ReadOnlyMemory<byte>>(
            "tgs.bo.notify",
            notifyEvent.ToByteArray(),
            cancellationToken).ConfigureAwait(false);
    }
}
