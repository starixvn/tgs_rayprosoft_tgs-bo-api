using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BrandGame;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

public partial class BrandGameController
{
    [HttpPut("{id}/{status}")]
    [Description("Change a brand game status")]
    public Task<MessageResponse> UpdateBrandStatusAsync(Guid id, GameStatus status) =>
        _mediator.Send(new UpdateBrandGameStatusRequest(id, status));
}
