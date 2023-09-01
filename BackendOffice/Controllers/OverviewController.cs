#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OverviewController : BaseContextController
{
    [HttpGet("netdeposit/{brandId}",Name = "GetCurrentDayNetDeposit")]
    public ValueTask<decimal> GetCurrentDayNetDepositAsync(
        [FromServices] IPaymentServiceClient paymentServiceClient,
        Guid brandId)
        => paymentServiceClient.GetBrandNetDepositAsync(brandId, HttpContext.RequestAborted);

    [HttpGet("successrate/{brandId}",Name = "GetCurrentDaySuccessRate")]
    public ValueTask<decimal> GetCurrentDaySuccessRateAsync(
        [FromServices] IPaymentServiceClient paymentServiceClient,
        Guid brandId)
        => paymentServiceClient.GetCurrentDayDepositSuccessRateAsync(brandId, HttpContext.RequestAborted);

    [HttpGet("countregplayers/{brandId}", Name = "GetCurrentDayRegisterPlayersCount")]
    public ValueTask<int> GetCurrentDayRegisterPlayersCountAsync(
        [FromServices] IPlayerServiceClient playerServiceClient,
        Guid brandId)
        => playerServiceClient.CountCurrentDayRegisterPlayersAsync(brandId, HttpContext.RequestAborted);
}
