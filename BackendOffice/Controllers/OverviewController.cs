#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices.Overview;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OverviewController : BaseContextController
{
    [HttpGet("player/login/count/{brandId}", Name = "GetCurrentDayLoginPlayersCount")]
    public ValueTask<int> GetCurrentDayLoginPlayersCountAsync(
       [FromServices] IPlayerServiceClient playerServiceClient,
       Guid brandId)
       => playerServiceClient.CountCurrentDayLoginPlayersAsync(brandId, HttpContext.RequestAborted);

    [HttpGet("net-deposit/{brandId}", Name = "GetDayNetDeposit")]
    public async ValueTask<NetDepositViewModel> GetDayNetDepositAsync(
        [FromServices] IGetService<GetDayNetDepositQuery, DayNetDepositInfo> getService,
        Guid brandId)
    {        
        var dayNetDepositInfo = await getService
            .GetAsync( new GetDayNetDepositQuery(brandId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new NetDepositViewModel
        {
            TodayNetDeposit = dayNetDepositInfo.Today,
            YesterdayNetDeposit = dayNetDepositInfo.Yesterday
        };     
    }

    [HttpGet("player/reg/count/{brandId}", Name = "GetCurrentDayRegisterPlayersCount")]
    public ValueTask<int> GetCurrentDayRegisterPlayersCountAsync(
        [FromServices] IPlayerServiceClient playerServiceClient,
        Guid brandId)
        => playerServiceClient.CountCurrentDayRegisterPlayersAsync(brandId, HttpContext.RequestAborted);

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
}
