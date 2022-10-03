using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.PlayerIp;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.PlayerIpHandlers;

public class GetPlayerIpByIpOperationQueryHandler : Handler<GetPlayerIpsByIpRequest, PagedResponse<PlayerIpByIpModel>>
{
    private readonly IPlayerIpService _service;

    public GetPlayerIpByIpOperationQueryHandler(IPlayerIpService service, IHttpContextAccessor accessor) :
        base(accessor)
    {
        _service = service ?? throw new System.ArgumentNullException(nameof(service));
    }

    public override Task<PagedResponse<PlayerIpByIpModel>> Handle(GetPlayerIpsByIpRequest request, CancellationToken cancellationToken)
    {
        var endDateTimeOffset = DateTimeOffset.UtcNow;
        var startDateTimeOffset = endDateTimeOffset.AddDays(-14);

        if (request.IpType == IpType.RegisterIp)
        {
            return _service
                .GetPlayerIpRecordsByRegisterQueries(request.BrandId, request.IpAddress, startDateTimeOffset, endDateTimeOffset)
                .CreatePagedResponseAsync(request);
        }
        else
        {
            return _service
                .GetPlayerIpOperationByIpQueries(request.BrandId, request.IpAddress, startDateTimeOffset, endDateTimeOffset)
                .CreatePagedResponseAsync(request);
        }
    }
}
