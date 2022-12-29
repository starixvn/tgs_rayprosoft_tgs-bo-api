using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.GameProvider;

namespace TGS.Apis.BackendOffice.Controllers.GameProvider;

public partial class GameProviderController
{
    [HttpPut("{id:guid}/{status}")]
    [Description("change a game provider status")]
    public Task<MessageResponse> UpdateGameProviderStatusAsync(Guid id, Status status) =>
        _mediator.Send(new UpdateGameProviderStatusRequest(id, status));
}
