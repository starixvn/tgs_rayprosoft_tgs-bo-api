using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.PlayerIp;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.PlayerIpHandlers;

public class GetPlayerIpByUserNameOperationQueryHandler : Handler<GetPlayerIpsByUserNameRequest, PagedResponse<PlayerIpByUserNameModel>>
{
    private readonly IPlayerIpService _service;

    public GetPlayerIpByUserNameOperationQueryHandler(IPlayerIpService service, IHttpContextAccessor accessor)
        : base(accessor)
    {
        _service = service ?? throw new System.ArgumentNullException(nameof(service));
    }

    public override Task<PagedResponse<PlayerIpByUserNameModel>> Handle(GetPlayerIpsByUserNameRequest request, CancellationToken cancellationToken)
    {
        var endDateTimeOffset = DateTimeOffset.UtcNow;
        var startDateTimeOffset = endDateTimeOffset.AddMonths(-6);

        var loginRecords = _service.GetPlayerIpOperationByUsernameQueries(request.BrandId, request.UserName, startDateTimeOffset, endDateTimeOffset);

        var registerRecords = _service
          .GetPlayerIpRecordsByRegister(request.BrandId, request.UserName)
          .Where(x => x.RecordOn > endDateTimeOffset && x.RecordOn < endDateTimeOffset)
          .Select(x => new PlayerIpByUserNameModel
          {
              IpAddress = x.Ip,
              CreatedOn = x.RecordOn
          })
          .OrderByDescending(x => x.CreatedOn);
        _ = loginRecords.Concat(registerRecords);

        return loginRecords.GroupBy(x => new
        {
            x.IpAddress
        }).Select(group => new PlayerIpByUserNameModel
        {
            IpAddress = group.Key.IpAddress,
            CreatedOn = group.Max(y => y.CreatedOn)
        }).OrderByDescending(x => x.CreatedOn)
          .CreatePagedResponseAsync(request);
    }
}
