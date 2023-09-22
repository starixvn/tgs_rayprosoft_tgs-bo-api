using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Tgs.BrandCertificate.Clients;
using Tgs.GameWagers.Client;
using Tgs.GameWagers.Common;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.Overview;

internal class GetTopTenPlayerQueryService : IQueryService<GetTopTenPlayerQuery, TopTenPlayerInfo>
{
    private readonly IBrandServiceClient _brandServiceClient;
    private readonly IGameWagersClient _gameWagersClient;
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly IPlayerServiceClient _playerServiceClient;

    public GetTopTenPlayerQueryService(
        IPaymentServiceClient paymentServiceClient,
        IBrandServiceClient brandServiceClient,
        IGameWagersClient gameWagersClient,
        IPlayerServiceClient playerServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
        _brandServiceClient = brandServiceClient ?? throw new ArgumentNullException(nameof(brandServiceClient));
        _gameWagersClient = gameWagersClient ?? throw new ArgumentNullException(nameof(gameWagersClient));
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
    }

    public async IAsyncEnumerable<TopTenPlayerInfo> QueryAsync(GetTopTenPlayerQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8D));
        var currentDay = new DateTimeOffset(now.Date, TimeSpan.FromHours(8D)).UtcDateTime;

        var brandInfo = await _brandServiceClient
            .GetBrandInfoAsync(query.BrandId, cancellationToken)
            .ConfigureAwait(false);

        switch (query.SortColumnType)
        {
            case TopTenPlayerSortType.Deposit:
                var topTenDeposits = await _paymentServiceClient
                    .GetCurrentDayTopTenDepositPlayersAsync(query.BrandId, cancellationToken)
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (!topTenDeposits.Any())
                    yield break;

                var playerCodes = topTenDeposits.Select(t => t.PlayerCode).ToArray();

                var wagerInfoDict = await _gameWagersClient
                    .BrandWagerPlayerStatsPlayerQueryAsync(brandInfo.Code, currentDay, now, playerCodes, cancellationToken)
                    .ToDictionaryAsync(w => w.PlayerCode, cancellationToken)
                    .ConfigureAwait(false);

                var playerInfoDict = await _playerServiceClient
                    .QueryPlayerInfoListAsync(query.BrandId, playerCodes, cancellationToken)
                    .ToDictionaryAsync(p => p.PlayerCode, cancellationToken)
                    .ConfigureAwait(false);

                foreach (var item in topTenDeposits)
                {
                    var IsPlayerExist = playerInfoDict.TryGetValue(item.PlayerCode, out var playerInfo);
                    var HasWager = wagerInfoDict.TryGetValue(item.PlayerCode, out var wagerInfo);

                    yield return new TopTenPlayerInfo
                    {
                        PlayerCode = item.PlayerCode,
                        PlayerUserName = IsPlayerExist ? playerInfo.UserName : string.Empty,
                        DepositTotalAmount = item.DepositTotalAmount,
                        WithdrawTotalAmount = item.WithdrawTotalAmount,
                        Bet = HasWager ? wagerInfo.Bet : 0m,
                        Settle = HasWager ? wagerInfo.Settle : 0m,
                        WinLoss = HasWager ? wagerInfo.WinLoss : 0m,
                    };
                }

                break;

            case TopTenPlayerSortType.Withdraw:
                var topTenWithdraws = await _paymentServiceClient
                    .GetCurrentDayTopTenWithdrawPlayersAsync(query.BrandId, cancellationToken)
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (!topTenWithdraws.Any())
                    yield break;

                playerCodes = topTenWithdraws.Select(t => t.PlayerCode).ToArray();

                wagerInfoDict = await _gameWagersClient
                    .BrandWagerPlayerStatsPlayerQueryAsync(brandInfo.Code, currentDay, now, playerCodes, cancellationToken)
                    .ToDictionaryAsync(w => w.PlayerCode, cancellationToken)
                    .ConfigureAwait(false);

                playerInfoDict = await _playerServiceClient
                    .QueryPlayerInfoListAsync(query.BrandId, playerCodes, cancellationToken)
                    .ToDictionaryAsync(p => p.PlayerCode, cancellationToken)
                    .ConfigureAwait(false);

                foreach (var item in topTenWithdraws)
                {
                    var IsPlayerExist = playerInfoDict.TryGetValue(item.PlayerCode, out var playerInfo);
                    var HasWager = wagerInfoDict.TryGetValue(item.PlayerCode, out var wagerInfo);

                    yield return new TopTenPlayerInfo
                    {
                        PlayerCode = item.PlayerCode,
                        PlayerUserName = IsPlayerExist ? playerInfo.UserName : string.Empty,
                        DepositTotalAmount = item.DepositTotalAmount,
                        WithdrawTotalAmount = item.WithdrawTotalAmount,
                        Bet = HasWager ? wagerInfo.Bet : 0m,
                        Settle = HasWager ? wagerInfo.Settle : 0m,
                        WinLoss = HasWager ? wagerInfo.WinLoss : 0m,
                    };
                }

                break;

            case TopTenPlayerSortType.Bet:
                var topTenBets = await _gameWagersClient
                    .BrandWagerPlayerStatsQueryAsync(
                        brandInfo.Code,
                        startOn: currentDay,
                        endOn: now,
                        BrandWagerPlayerMinsStatsSort.Bet,
                        false,
                        page: 1,
                        pageSize: 10,
                        cancellationToken: cancellationToken)
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (!topTenBets.Any())
                    yield break;

                playerCodes = topTenBets.Select(x => x.PlayerCode).ToArray();

                var paymentDict = await _paymentServiceClient
                    .GetCurrentDayDepositWithdrawTotalAmountAsync(playerCodes, cancellationToken)
                    .ToDictionaryAsync(t => t.PlayerCode, cancellationToken)
                    .ConfigureAwait(false);

                playerInfoDict = await _playerServiceClient
                    .QueryPlayerInfoListAsync(query.BrandId, playerCodes, cancellationToken)
                    .ToDictionaryAsync(p => p.PlayerCode, cancellationToken)
                    .ConfigureAwait(false);

                foreach (var item in topTenBets)
                {
                    var HasPaymentData = paymentDict.TryGetValue(item.PlayerCode, out var paymentInfo);
                    var IsPlayer = playerInfoDict.TryGetValue(item.PlayerCode, out var playerInfo);

                    yield return new TopTenPlayerInfo
                    {
                        Bet = item.Bet,
                        Settle = item.Settle,
                        WinLoss = item.WinLoss,
                        DepositTotalAmount = HasPaymentData ? paymentInfo.DepositTotalAmount : 0m,
                        WithdrawTotalAmount = HasPaymentData ? paymentInfo.WithdrawTotalAmount : 0m,
                        PlayerUserName = IsPlayer ? playerInfo.UserName : string.Empty,
                        PlayerCode = item.PlayerCode,
                    };
                }

                break;

            case TopTenPlayerSortType.Settle:
                var topTenSettle = await _gameWagersClient
                    .BrandWagerPlayerStatsQueryAsync(
                        brandInfo.Code,
                        startOn: currentDay,
                        endOn: now,
                        BrandWagerPlayerMinsStatsSort.Settle,
                        false,
                        page: 1,
                        pageSize: 10,
                        cancellationToken: cancellationToken)
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (!topTenSettle.Any())
                    yield break;

                playerCodes = topTenSettle.Select(x => x.PlayerCode).ToArray();

                paymentDict = await _paymentServiceClient
                    .GetCurrentDayDepositWithdrawTotalAmountAsync(playerCodes, cancellationToken)
                    .ToDictionaryAsync(t => t.PlayerCode, cancellationToken)
                    .ConfigureAwait(false);

                playerInfoDict = await _playerServiceClient
                    .QueryPlayerInfoListAsync(query.BrandId, playerCodes, cancellationToken)
                    .ToDictionaryAsync(p => p.PlayerCode, cancellationToken)
                    .ConfigureAwait(false);

                foreach (var item in topTenSettle)
                {
                    var HasPaymentData = paymentDict.TryGetValue(item.PlayerCode, out var paymentInfo);
                    var IsPlayer = playerInfoDict.TryGetValue(item.PlayerCode, out var playerInfo);

                    yield return new TopTenPlayerInfo
                    {
                        Bet = item.Bet,
                        Settle = item.Settle,
                        WinLoss = item.WinLoss,
                        DepositTotalAmount = HasPaymentData ? paymentInfo.DepositTotalAmount : 0m,
                        WithdrawTotalAmount = HasPaymentData ? paymentInfo.WithdrawTotalAmount : 0m,
                        PlayerUserName = IsPlayer ? playerInfo.UserName : string.Empty,
                        PlayerCode = item.PlayerCode,
                    };
                }
                break;

            case TopTenPlayerSortType.WinLoss:

                var topTenWinLoss = await _gameWagersClient
                    .BrandWagerPlayerStatsQueryAsync(
                        brandInfo.Code,
                        startOn: currentDay,
                        endOn: now,
                        BrandWagerPlayerMinsStatsSort.WinLoss,
                        isAscending: query.Ascending,
                        page: 1,
                        pageSize: 10,
                        cancellationToken: cancellationToken)
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (!topTenWinLoss.Any())
                    yield break;

                playerCodes = topTenWinLoss.Select(x => x.PlayerCode).ToArray();

                paymentDict = await _paymentServiceClient
                    .GetCurrentDayDepositWithdrawTotalAmountAsync(playerCodes, cancellationToken)
                    .ToDictionaryAsync(t => t.PlayerCode, cancellationToken)
                    .ConfigureAwait(false);

                playerInfoDict = await _playerServiceClient
                    .QueryPlayerInfoListAsync(query.BrandId, playerCodes, cancellationToken)
                    .ToDictionaryAsync(p => p.PlayerCode, cancellationToken)
                    .ConfigureAwait(false);

                foreach (var item in topTenWinLoss)
                {
                    var HasPaymentData = paymentDict.TryGetValue(item.PlayerCode, out var paymentInfo);
                    var IsPlayer = playerInfoDict.TryGetValue(item.PlayerCode, out var playerInfo);

                    yield return new TopTenPlayerInfo
                    {
                        Bet = item.Bet,
                        Settle = item.Settle,
                        WinLoss = item.WinLoss,
                        DepositTotalAmount = HasPaymentData ? paymentInfo.DepositTotalAmount : 0m,
                        WithdrawTotalAmount = HasPaymentData ? paymentInfo.WithdrawTotalAmount : 0m,
                        PlayerUserName = IsPlayer ? playerInfo.UserName : string.Empty,
                        PlayerCode = item.PlayerCode,
                    };
                }

                break;

            default:
                break;
        }
    }
}
