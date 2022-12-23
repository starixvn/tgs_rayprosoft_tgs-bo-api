using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.VipLevel;

public class GetVipLevelsRequest : PageableRequest, IRequest<PagedResponse<VipLevelModel>>
{
    public Guid? BrandId { get; set; }
}
