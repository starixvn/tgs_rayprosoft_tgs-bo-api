using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

public class UpdateBrandGameProviderStatusRequest(Guid id, Status status) : BaseRequest, IRequest<MessageResponse>
{
    public Guid Id { get; set; } = id;

    public Status Status { get; set; } = status;
}
