using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Filters;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices.Overview;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OverviewController : BaseContextController
{
    [BOTypeFilter(typeof(MerchanPermissionRouteFiler), Arguments = ["brand", "brandId"])]
    [HttpGet("player/login/count/{brandId}", Name = "GetCurrentDayLoginPlayersCount")]
    public ValueTask<int> GetCurrentDayLoginPlayersCountAsync(
       [FromServices] IPlayerServiceClient playerServiceClient,
       Guid brandId)
       => playerServiceClient.CountCurrentDayLoginPlayersAsync(brandId, HttpContext.RequestAborted);

    [BOTypeFilter(typeof(MerchanPermissionRouteFiler), Arguments = ["brand", "brandId"])]
    [HttpGet("net-deposit/{brandId}", Name = "GetDayNetDeposit")]
    public async ValueTask<NetDepositViewModel> GetDayNetDepositAsync(
        [FromServices] IGetService<GetDayNetDepositQuery, DayNetDepositInfo> getService,
        Guid brandId)
    {
        var dayNetDepositInfo = await getService
            .GetAsync(new GetDayNetDepositQuery(brandId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new NetDepositViewModel
        {
            TodayNetDeposit = dayNetDepositInfo.Today,
            YesterdayNetDeposit = dayNetDepositInfo.Yesterday
        };
    }

    [BOTypeFilter(typeof(MerchanPermissionRouteFiler), Arguments = ["brand", "brandId"])]
    [HttpGet("success-rate/{brandId}", Name = "GetDepositDaySuccessRate")]
    public async ValueTask<DayDepositSuccessRateViewModel> GetDepositDaySuccessRateAsync(
        [FromServices] IGetService<GetDaySuccessRateQuery, DaySuccessRateInfo> getService,
        Guid brandId)
    {
        var daySuccessRateInfo = await getService
            .GetAsync(new GetDaySuccessRateQuery(brandId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new DayDepositSuccessRateViewModel
        {
            TodaySuccessRate = daySuccessRateInfo.Today,
            YesterdaySuccessRate = daySuccessRateInfo.Yesterday
        };
    }

    [BOTypeFilter(typeof(MerchanPermissionRouteFiler), Arguments = ["brand", "brandId"])]
    [HttpGet("player/reg/count/{brandId}", Name = "GetRegisterPlayersCount")]
    public async ValueTask<DayPlayerRegisterCountViewModel> GetRegisterPlayersCountAsync(
        [FromServices] IGetService<GetDayPlayerRegisterCountQuery, DayPlayerRegisterCountInfo> getService,
        Guid brandId)
    {
        var info = await getService
            .GetAsync(new GetDayPlayerRegisterCountQuery(brandId), HttpContext.RequestAborted)
            .ConfigureAwait(false);
        return new DayPlayerRegisterCountViewModel
        {
            TodayPlayerRegisterCount = info.Today,
            YesterdayPlayerRegisterCount = info.Yesterday,
        };
    }

    [BOTypeFilter(typeof(MerchanPermissionRouteFiler), Arguments = ["brand", "brandId"])]
    [HttpGet("top-ten/game/{brandId}")]
    public async IAsyncEnumerable<TopTenGameViewModel> GetTopTenGamesAsync(
        [FromServices] IQueryService<GetTopTenGameQuery, TopTenGameInfo> queryService,
        [FromServices] IQueryService<GetTopGameInfoQuery, TopGameInfo> gameQueryService,
        Guid brandId,
        [FromQuery] TopTenGameSortType sortType,
        [FromQuery] bool ascending = false)
    {
        var topTenGameInfos = await queryService
            .QueryAsync(new GetTopTenGameQuery(brandId, sortType, ascending), HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var gameProviderIds = topTenGameInfos.Select(x => x.GameProviderId);
        var gameIds = topTenGameInfos.Select(x => x.GameId);

        var gameInfos = await gameQueryService
            .QueryAsync(new GetTopGameInfoQuery(gameProviderIds, gameIds), HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var cultureCode = CultureInfo.CurrentUICulture.Name;

        foreach (var info in topTenGameInfos)
        {
            var gameInfo = gameInfos
                .Where(x => x.GameProviderId == info.GameProviderId
                    && x.GameId == info.GameId)
                .First();

            _ = gameInfo!.LocalizationDicts.TryGetValue(cultureCode, out var locale);

            yield return new TopTenGameViewModel
            {
                GameName = locale?.Name ?? gameInfo.GameName,
                GameProviderName = gameInfo.SubGameProviderName,
                Bet = info.Bet,
                WinLoss = info.WinLoss,
            };
        }
    }

    [BOTypeFilter(typeof(MerchanPermissionRouteFiler), Arguments = ["brand", "brandId"])]
    [HttpGet("top-ten/{brandId}")]
    public async IAsyncEnumerable<TopTenPlayerViewModel> GetTopTenPlayersAsync(
        [FromServices] IQueryService<GetTopTenPlayerQuery, TopTenPlayerInfo> queryService,
        Guid brandId,
        [FromQuery] TopTenPlayerSortType sortType,
        [FromQuery] bool ascending = false)
    {
        var infos = await queryService
            .QueryAsync(new GetTopTenPlayerQuery(brandId, sortType, ascending), HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        foreach (var info in infos)
        {
            yield return new TopTenPlayerViewModel
            {
                PlayerCode = info.PlayerCode,
                PlayerUsername = info.PlayerUserName,
                Deposit = info.DepositTotalAmount,
                Withdraw = info.WithdrawTotalAmount,
                Bet = info.Bet,
                Settle = info.Settle,
                WinLoss = info.WinLoss,
            };
        }
    }
}