using System;
using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.Game;

public class CreateGamesRequest : BaseRequest, IRequest<MessageResponse>
{
    public Guid GameProviderId { get; set; }

    public GameModel[] Games { get; set; }
}
