using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BrandGame;

public class UpdateBrandGameStatusRequest(Guid id, GameStatus status) : IRequest<MessageResponse>
{
    public Guid Id { get; set; } = id;

    public GameStatus Status { get; set; } = status;
}
