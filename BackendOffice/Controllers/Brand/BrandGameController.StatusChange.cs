using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.BrandGame;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

public partial class BrandGameController
{
    [HttpPut("{id}/{status}")]
    [Description("Change a brand game status")]
    public Task<MessageResponse> UpdateBrandStatusAsync(Guid id, GameStatus status) =>
        mediator.Send(new UpdateBrandGameStatusRequest(id, status));
}
