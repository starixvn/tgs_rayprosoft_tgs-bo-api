using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.OfflineLoginReward;

internal class ProcessOfflineLoginRewardTempUserBatchCommandService
    : ICommandService<ProcessOfflineLoginRewardTempUserBatchCommand>
{
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

        var segments = batch.UserNames
            .Distinct()
            .Select((userName, index) =>
                new
                {
                    UserName = userName,
                    Group = index / _SegmentCount
                })
            .GroupBy(x => x.Group, x => x.UserName)
            .Select(g => g.ToArray())
            .ToArray();
        
        var userNameMap = new ConcurrentDictionary<string, string>();

        var tasks = segments.Select((userNames, idx) =>
            Task.Run(async () =>
            {
                var query = new PlayerInfosByUserNamesQuery(batch.BrandId, userNames);

                var infos = await _playerInfosQueryService.QueryAsync(query, cancellationToken)
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);
                
                foreach (var info in infos)
                    _ = userNameMap.TryAdd(info.UserName, info.PlayerCode);

            }, cancellationToken));

        await Task.WhenAll(tasks).ConfigureAwait(false);
        
        var playerItems = batch.UserNames
            .Select((userName, idx) =>
            {
                if (!userNameMap.TryGetValue(userName, out var playerCode))
                    playerCode = string.Empty;

                return new OfflineLoginRewardPlayerItem(playerCode,
                    userName);
            });

        await _batchTempRepository
            .SaveOfflineLoginRewardPlayerItemsAsync(command.BatchId, playerItems, cancellationToken)
            .ConfigureAwait(false);

        await _batchTempRepository
            .CompleteOfflineLoginRewardBackgroundProcessAsync(command.BatchId, cancellationToken)
            .ConfigureAwait(false);
    }
}