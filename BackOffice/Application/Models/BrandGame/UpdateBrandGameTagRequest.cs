using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BrandGame;

public class UpdateBrandGameTagRequest : IRequest<MessageResponse>
{
    public bool Enabled { get; set; }

    public GameTag GameTag { get; set; }

    public Guid Id { get; set; }

    public UpdateBrandGameTagRequest(Guid id, GameTag tag, bool enabled)
    {
        Id = id;
        GameTag = tag;
        Enabled = enabled;
    }
}
