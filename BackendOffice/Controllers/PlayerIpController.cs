using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.PlayerIp;
using TGS.Services.BackOffice.ApplicationServices.Players;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlayerIpController(IMediator mediator) : BaseContextController
{
    [HttpGet("Ip")]
    [Description("Get player ipaddress By IP ")]
    public Task<PagedResponse<PlayerIpByIpModel>> GetPlayerIpsByIpListAsync([FromQuery] GetPlayerIpsByIpRequest request)
        => mediator.Send(request);

    [HttpGet("username")]
    [Description("Get player ipaddress By UserName")]
    public ValueTask<PagedResponse<GetPlayerIpsByUserNameResult>> GetPlayerIpsByUserNameListAsync(
        [FromServices] IGetService<GetPlayerIpsByUserNameQuery, PagedResponse<GetPlayerIpsByUserNameResult>> service,
        [FromQuery] GetPlayerIpsByUserNameQuery source)
        => service.GetAsync(
            query: source,
            cancellationToken: HttpContext.RequestAborted);

    [HttpGet]
    [Description("Get player ipaddress ")]
    public Task<PagedResponse<PlayerIpModel>> GetPlayerIpsListAsync([FromQuery] GetPlayerIpsRequest request) =>
        mediator.Send(request);
}
