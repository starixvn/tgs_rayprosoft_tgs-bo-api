using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Services.BackOffice.Application.Models.SubGameProvider;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.GameProvider;

[Route("api/[controller]")]
[ApiController]
public class SubGameProviderController(IMediator mediator) : BaseContextController
{
    [HttpPost]
    [Description("Create a sub game provider")]
    public Task<MessageResponse> CreateGameProviderAsync(CreateSubGameProviderRequest request) =>
        mediator.Send(request);

    [HttpGet("{code}")]
    [Description("Get a sub game provider")]
    public Task<GetSubGameProviderResponse> GetSubGameProviderAsync(string code) =>
        mediator.Send(new GetSubGameProviderRequest(code));

    [HttpGet]
    [Description("Get sub game providers")]
    public Task<PagedResponse<SubGameProviderModel>>
        GetSubGameProvidersAsync([FromQuery] GetSubGameProvidersRequest request) => mediator.Send(request);

    [HttpPut("{id}/{code}")]
    [Description("Update a sub game provider")]
    public Task<MessageResponse> UpdateGameProviderAsync(Guid id, string code, UpdateSubGameProviderRequest request)
    {
        request.GameProviderId = id;
        request.Code = code;
        return mediator.Send(request);
    }
}
