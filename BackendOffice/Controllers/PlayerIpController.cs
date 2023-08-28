using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.PlayerIp;
using TGS.Services.BackOffice.ApplicationServices.Players;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlayerIpController : BaseContextController
{
    private readonly IMediator _mediator;

    public PlayerIpController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("Ip")]
    [Description("Get player ipaddress By IP ")]
    public Task<PagedResponse<PlayerIpByIpModel>> GetPlayerIpsByIpListAsync([FromQuery] GetPlayerIpsByIpRequest request) =>
        _mediator.Send(request);

    [HttpGet("UserName")]
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
        _mediator.Send(request);
}
