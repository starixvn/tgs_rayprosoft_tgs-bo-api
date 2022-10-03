using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

public class UpdateBrandGameProviderStatusRequest : BaseRequest, IRequest<MessageResponse>
{
    public UpdateBrandGameProviderStatusRequest(Guid id, Status status)
    {
        Id = id;
        Status = status;
    }

    public Guid Id { get; set; }
    public Status Status { get; set; }
}