using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BrandGame;

public class UpdateBrandGameStatusRequest : IRequest<MessageResponse>
{
    public UpdateBrandGameStatusRequest(Guid id, GameStatus status)
    {
        Id = id;
        Status = status;
    }

    public Guid Id { get; set; }
    public GameStatus Status { get; set; }
}