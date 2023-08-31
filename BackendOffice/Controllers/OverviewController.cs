#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.Clients;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OverviewController : BaseContextController
{
    [HttpGet("successrate/{brandId}")]
    public ValueTask<decimal> GetCurrentDaySuccessRateAsync(
        [FromServices] IPaymentServiceClient paymentServiceClient,
        Guid brandId)
        => paymentServiceClient.GetCurrentDayDepositSuccessRateAsync(brandId, HttpContext.RequestAborted);
}
