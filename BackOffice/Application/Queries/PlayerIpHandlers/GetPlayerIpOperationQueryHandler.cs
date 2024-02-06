using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.PlayerIp;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.PlayerIpHandlers;

public class GetPlayerIpOperationQueryHandler(
    IPlayerIpService service,
    IHttpContextAccessor accessor)
    : Handler<GetPlayerIpsRequest, PagedResponse<PlayerIpModel>>(accessor)
{
    private readonly IPlayerIpService _service = service ?? throw new System.ArgumentNullException(nameof(service));

    public override Task<PagedResponse<PlayerIpModel>> Handle(GetPlayerIpsRequest request, CancellationToken cancellationToken)
    {
        var endDateTimeOffset = DateTimeOffset.UtcNow;
        var startDateTimeOffset = endDateTimeOffset.AddDays(-14);

        return request.IpType == IpType.RegisterIp
            ? _service
                .GetPlayerIpRecordsByRegisterQueries(request.BrandId, startDateTimeOffset, endDateTimeOffset)
                .CreatePagedResponseAsync(request)
            : _service
                .GetPlayerIpOperationQueries(request.BrandId, startDateTimeOffset, endDateTimeOffset)
                .CreatePagedResponseAsync(request);
    }
}
