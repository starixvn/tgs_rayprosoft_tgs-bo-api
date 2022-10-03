using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.GameProvider;

public class UpdateGameProviderStatusRequest : BaseRequest, IRequest<MessageResponse>
{
    public UpdateGameProviderStatusRequest(Guid id, Status status)
    {
        Id = id;
        Status = status;
    }

    public Guid Id { get; set; }
    public Status Status { get; set; }
}