using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.PlayerIp;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.PlayerIpHandlers;

public class GetPlayerIpByIpOperationQueryHandler(
    IPlayerIpService service,
    IHttpContextAccessor accessor)
    : Handler<GetPlayerIpsByIpRequest, PagedResponse<PlayerIpByIpModel>>(accessor)
{
    public override Task<PagedResponse<PlayerIpByIpModel>> Handle(GetPlayerIpsByIpRequest request, CancellationToken cancellationToken)
    {
        var endDateTimeOffset = DateTimeOffset.UtcNow;
        var startDateTimeOffset = endDateTimeOffset.AddDays(-14);

        return request.IpType == IpType.RegisterIp
            ? service
                .GetPlayerIpRecordsByRegisterQueries(request.BrandId, request.IpAddress, startDateTimeOffset, endDateTimeOffset)
                .CreatePagedResponseAsync(request)
            : service
                .GetPlayerIpOperationByIpQueries(request.BrandId, request.IpAddress, startDateTimeOffset, endDateTimeOffset)
                .CreatePagedResponseAsync(request);
    }
}
