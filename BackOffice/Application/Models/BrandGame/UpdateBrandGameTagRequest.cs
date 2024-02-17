using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.BrandGame;

public class UpdateBrandGameTagRequest(Guid id, GameTag tag, bool enabled) : IRequest<MessageResponse>
{
    public bool Enabled { get; set; } = enabled;

    public GameTag GameTag { get; set; } = tag;

    public Guid Id { get; set; } = id;
}
