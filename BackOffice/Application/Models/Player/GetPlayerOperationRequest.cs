using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class GetPlayerOperationRequest : PageableRequest, IRequest<PagedResponse<PlayerOperationModel>>
{
    public string PlayerCode { get; set; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}