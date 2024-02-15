using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Services.BackOffice.Application.Models.BetLimit;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.BetLimit;

[Route("api/[controller]")]
[ApiController]
public class BetLimitController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Description("Create a bet limit")]
    public Task<MessageResponse> CreateBetLimitAsync(CreateBetLimitRequest request) => mediator.Send(request);

    [HttpGet("{id}")]
    [Description("Get a bet limit")]
    public Task<GetBetLimitResponse> GetBetLimitAsync(int id) => mediator.Send(new GetBetLimitRequest(id));

    [HttpGet]
    [Description("Get bet limits")]
    public Task<PagedResponse<BetLimitModel>> GetBetLimitsAsync([FromQuery] GetBetLimitsRequest request) =>
        mediator.Send(request);

    [HttpPut("{id}")]
    [Description("Update a bet limit")]
    public Task<MessageResponse> UpdateBetLimitAsync(int id, UpdateBetLimitRequest request)
    {
        request.Id = id;
        return mediator.Send(request);
    }
}
