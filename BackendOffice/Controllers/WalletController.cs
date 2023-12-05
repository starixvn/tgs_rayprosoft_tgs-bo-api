#nullable enable

using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices.Players;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WalletController : BaseContextController
{
    [HttpGet("~/api/player/{playerCode}/mainwallet", Name = "GetPlayerMainWalletBalanceOld")]
    [HttpGet("player/{playerCode}", Name = "GetPlayerMainWalletBalance")]
    [Description("Get Player Main wallet")]
    public ValueTask<decimal> GetPlayerMainWalletBalanceAsync(
        [FromServices] IGetService<GetPlayerBalanceQuery, decimal> service,
        string playerCode)
        => service.GetAsync(new(playerCode));
}
