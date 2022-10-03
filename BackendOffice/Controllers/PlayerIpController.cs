using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.PlayerIp;

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

    [HttpGet]
    [Description("Get player ipaddress ")]
    public Task<PagedResponse<PlayerIpModel>> GetPlayerIpsListAsync([FromQuery] GetPlayerIpsRequest request) =>
        _mediator.Send(request);

    [HttpGet("Ip")]
    [Description("Get player ipaddress By IP ")]
    public Task<PagedResponse<PlayerIpByIpModel>> GetPlayerIpsByIpListAsync([FromQuery] GetPlayerIpsByIpRequest request) =>
        _mediator.Send(request);

    [HttpGet("UserName")]
    [Description("Get player ipaddress By UserName")]
    public Task<PagedResponse<PlayerIpByUserNameModel>> GetPlayerIpsByUserNameListAsync([FromQuery] GetPlayerIpsByUserNameRequest request) =>
        _mediator.Send(request);
}
