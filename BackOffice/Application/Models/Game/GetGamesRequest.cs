using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.Game;

public class GetGamesRequest : PageableRequest, IRequest<PagedResponse<GameModel>>
{
    public string Code { get; set; }

    public Guid GameProviderId { get; set; }

    public string Platform { get; set; }

    public string Status { get; set; }

    public GameTag Tag { get; set; }

}
