using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.Domain;
using Tgs.ServiceUtility;
using System.Collections.Generic;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal class ProcessOfflineLoginRewardTempUserBatchCommandService 
    : ICommandService<ProcessOfflineLoginRewardTempUserBatchCommand>
{
    private static readonly object _Locker = new();
    private static readonly int _SegmentCount = 500;

    private readonly IQueryService<PlayerInfosByUserNamesQuery, PlayerInfo> _playerInfosQueryService;
    private readonly IBatchTempRepository _batchTempRepository;
 
    public ProcessOfflineLoginRewardTempUserBatchCommandService(
        IQueryService<PlayerInfosByUserNamesQuery, PlayerInfo> playerInfosQueryService,
        IBatchTempRepository batchTempRepository)
    {
        _playerInfosQueryService = playerInfosQueryService ?? throw new ArgumentNullException(nameof(playerInfosQueryService));
        _batchTempRepository = batchTempRepository ?? throw new ArgumentNullException(nameof(batchTempRepository));
    }
    
    public async ValueTask ExecuteAsync(ProcessOfflineLoginRewardTempUserBatchCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!await _batchTempRepository
                .BidOfflineLoginRewardBackgroundProcessAsync(command.BatchId, cancellationToken)
                .ConfigureAwait(false))
            return;

        var batch = await _batchTempRepository
            .GetOfflineLoginRewardTempUserBatchAsync(command.BatchId, cancellationToken)
            .ConfigureAwait(false);

        if (batch is null)
        {
            await _batchTempRepository
                .CompleteOfflineLoginRewardBackgroundProcessAsync(command.BatchId, cancellationToken)
                .ConfigureAwait(false);

            return;
        }

        var segments = batch.UserNames.Select((userName, index) =>
                new
                {
                    UserName = userName,
                    Group = index / _SegmentCount
                })
            .GroupBy(x => x.Group, x => x.UserName)
            .Select(g => g.ToArray())
            .ToArray();

        var playerInfos = new List<PlayerInfo>();

        var tasks = segments.Select((userNames, idx) =>
            Task.Run(async () =>
            {
                var query = new PlayerInfosByUserNamesQuery(batch.BrandId, userNames);

                var infos = await _playerInfosQueryService.QueryAsync(query, cancellationToken)
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);

                lock (_Locker)
                    playerInfos.AddRange(infos);

            }, cancellationToken));

        await Task.WhenAll(tasks).ConfigureAwait(false);

        var playerItems =
            from item in batch.UserNames
                .Select((userName, idx) => new
                {
                    UserName = userName,
                    Index = idx + 1
                })
            join info in playerInfos on item.UserName equals info.UserName into g
            from r in g.DefaultIfEmpty()
            select new OfflineLoginRewardPlayerItem(
                item.Index,
                r?.PlayerCode ?? string.Empty,
                item.UserName);

        await _batchTempRepository
            .SaveOfflineLoginRewardPlayerItemsAsync(command.BatchId, playerItems, cancellationToken)
            .ConfigureAwait(false);

        await _batchTempRepository
            .CompleteOfflineLoginRewardBackgroundProcessAsync(command.BatchId, cancellationToken)
            .ConfigureAwait(false);
    }
}