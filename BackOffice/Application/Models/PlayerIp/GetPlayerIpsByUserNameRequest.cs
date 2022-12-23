using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.PlayerIp;

public class GetPlayerIpsByUserNameRequest : PageableRequest, IRequest<PagedResponse<PlayerIpByUserNameModel>>
{
    public Guid BrandId { get; init; }

    public string UserName { get; init; }
}
