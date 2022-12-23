using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Brand;

public class UpdateBrandStatusRequest : BaseRequest, IRequest<MessageResponse>
{
    public Guid BrandId { get; set; }

    public BrandStatus Status { get; set; }

    public UpdateBrandStatusRequest(Guid brandId, BrandStatus status)
    {
        BrandId = brandId;
        Status = status;
    }
}
