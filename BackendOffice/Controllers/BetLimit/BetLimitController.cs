using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BetLimit;

namespace TGS.Apis.BackendOffice.Controllers.BetLimit;

[Route("api/[controller]")]
[ApiController]
public class BetLimitController : ControllerBase
{
    private readonly IMediator _mediator;

    public BetLimitController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Description("Create a bet limit")]
    public Task<MessageResponse> CreateBetLimitAsync(CreateBetLimitRequest request) => _mediator.Send(request);

    [HttpGet("{id}")]
    [Description("Get a bet limit")]
    public Task<GetBetLimitResponse> GetBetLimitAsync(int id) => _mediator.Send(new GetBetLimitRequest(id));

    [HttpGet]
    [Description("Get bet limits")]
    public Task<PagedResponse<BetLimitModel>> GetBetLimitsAsync([FromQuery] GetBetLimitsRequest request) =>
        _mediator.Send(request);

    [HttpPut("{id}")]
    [Description("Update a bet limit")]
    public Task<MessageResponse> UpdateBetLimitAsync(int id, UpdateBetLimitRequest request)
    {
        request.Id = id;
        return _mediator.Send(request);
    }
}
