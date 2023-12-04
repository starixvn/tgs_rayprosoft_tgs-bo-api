using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models;
using TGS.Services.BackOffice.Application.Models.BrandGame;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

[Route("api/[controller]")]
[ApiController]
public partial class BrandGameController(IMediator mediator) : BaseContextController
{
    [HttpPut("ChangeOrders")]
    [Description("Change brand game order")]
    public Task<MessageResponse> ChangeOrderAsync(ChangeOrderRequest request) => mediator.Send(request);

    [HttpGet("{id}")]
    [Description("Get a brand game")]
    public Task<GetBrandGameResponse> GetBrandGameAsync(Guid id) => mediator.Send(new GetBrandGameRequest(id));

    [HttpGet]
    [Description("Get brands: 平台商遊戲列表")]
    public Task<PagedResponse<BrandGameModel>> GetBrandGamesAsync([FromQuery] GetBrandGamesRequest request) =>
        mediator.Send(request);

    [HttpPut("{id}")]
    [Description("Update a brand game")]
    public Task<MessageResponse> UpdateBrandGameAsync(Guid id, UpdateBrandGameRequest request)
    {
        request.Id = id;
        return mediator.Send(request);
    }

    [HttpPut("{id}/{tag}/{enabled:bool}")]
    [Description("Change a brand game tag")]
    public Task<MessageResponse> UpdateBrandTagAsync(Guid id, GameTag tag, bool enabled) =>
        mediator.Send(new UpdateBrandGameTagRequest(id, tag, enabled));
}
