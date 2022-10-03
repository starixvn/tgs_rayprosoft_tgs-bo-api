using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.PlayerIp;

public class GetPlayerIpsRequest : PageableRequest, IRequest<PagedResponse<PlayerIpModel>>
{
    public Guid BrandId { get; init; }
    public IpType IpType { get; init; }
}
