using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.GameProvider;

public class UpdateGameProviderStatusRequest(Guid id, Status status) : BaseRequest, IRequest<MessageResponse>
{
    public Guid Id { get; set; } = id;

    public Status Status { get; set; } = status;
}
