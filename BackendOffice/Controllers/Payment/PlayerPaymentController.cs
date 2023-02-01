using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Payment;

[Route("api/player/payment")]
[ApiController]
public class PlayerPaymentController : BaseContextController
{
    [HttpGet("{playerCode}")]
    public async Task<PaymentOverviewViewModel> GetPlayerPaymentOverviewAsync(
        [FromServices] IGetService<GetPlayerPaymentOverviewQuery, PlayerPaymentOverview> getService,
        string playerCode)
    {
        var query = new GetPlayerPaymentOverviewQuery(playerCode);

        var result = await getService.GetAsync(
            query,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new PaymentOverviewViewModel
        {
            FirstDepositAmount = result.FirstDepositAmount,
            FirstDepositHandlingFee = result.FirstDepositHandlingFee,
            FirstDepositOn = result.FirstDepositOn,
            FirstWithdrawAmount = result.FirstWithdrawAmount,
            FirstWithdrawHandlingFee = result.FirstWithdrawHandlingFee,
            FirstWithdrawOn = result.FirstWithdrawOn,
            RecordedBy = result.RecordedBy,
            TotalDepositAmount = result.TotalDepositAmount,
            TotalWithdrawAmount = result.TotalWithdrawAmount
        };
    }
}
