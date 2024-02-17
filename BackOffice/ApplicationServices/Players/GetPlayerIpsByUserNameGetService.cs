using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class GetPlayerIpsByUserNameGetService(IPlayerIpService service)
    : IGetService<GetPlayerIpsByUserNameQuery, PagedResponse<GetPlayerIpsByUserNameResult>>
{
    private readonly IPlayerIpService _service = service ?? throw new ArgumentNullException(nameof(service));

    public async ValueTask<PagedResponse<GetPlayerIpsByUserNameResult>> GetAsync(GetPlayerIpsByUserNameQuery query, CancellationToken cancellationToken = default)
    {
        var endDateTimeOffset = DateTimeOffset.UtcNow;
        var startDateTimeOffset = endDateTimeOffset.AddMonths(-6);
        var loginRecords = _service.GetPlayerIpOperationByUsernameQueries(query.BrandId, query.UserName, startDateTimeOffset, endDateTimeOffset);
        var registerRecords = _service.GetPlayerIpRecordsByRegister(query.BrandId, query.UserName)
            .Where(x => x.RecordOn > endDateTimeOffset && x.RecordOn < endDateTimeOffset)
            .Select(x => new GetPlayerIpsByUserNameResult
            {
                IpAddress = x.Ip,
                CreatedOn = x.RecordOn
            })
            .OrderByDescending(x => x.CreatedOn);

        return await loginRecords
            .GroupBy(x => new
            {
                x.IpAddress
            })
            .Select(group => new GetPlayerIpsByUserNameResult
            {
                IpAddress = group.Key.IpAddress,
                CreatedOn = group.Max(y => y.CreatedOn)
            })
            .OrderByDescending(x => x.CreatedOn)
            .CreatePagedResponseAsync(query)
            .ConfigureAwait(false);
    }
}
