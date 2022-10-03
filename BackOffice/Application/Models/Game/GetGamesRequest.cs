using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Game;

public class GetGamesRequest : PageableRequest, IRequest<PagedResponse<GameModel>>
{
    public Guid GameProviderId { get; set; }
    public string Code { get; set; }
}