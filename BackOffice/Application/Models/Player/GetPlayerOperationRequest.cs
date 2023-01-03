using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class GetPlayerOperationRequest : PageableRequest, IRequest<PagedResponse<PlayerOperationModel>>
{
    public DateTimeOffset From { get; init; }

    public string PlayerCode { get; set; }

    public DateTimeOffset To { get; init; }
}
