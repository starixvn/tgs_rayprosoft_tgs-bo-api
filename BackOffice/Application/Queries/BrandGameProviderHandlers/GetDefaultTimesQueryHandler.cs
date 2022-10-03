using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.BrandGameProviderHandlers;

public class
    GetDefaultTimesQueryHandler : Handler<GetDefaultTimesRequest,
        GetDefaultTimesResponse>
{
    private readonly IBrandGameProviderScheduleService _service;

    public GetDefaultTimesQueryHandler(IBrandGameProviderScheduleService service, IHttpContextAccessor accessor) :
        base(accessor)
    {
        _service = service;
    }

    public override async Task<GetDefaultTimesResponse> Handle(
        GetDefaultTimesRequest request,
        CancellationToken cancellationToken)
    {
        var defaultTimes = await _service.GetBrandGameProviderScheduleQueries(q => q
                .Where(ResellerId != null, r => r.BrandGameProvider.Brand.Licensee.ResellerId == ResellerId)
                .Where(LicenseeId != null, l => l.BrandGameProvider.Brand.LicenseeId == LicenseeId)
                .Where(BrandId != null, b => b.BrandGameProvider.BrandId == BrandId)
                .Where(x => x.BrandGameProviderId == request.BrandGameProviderId)
                .Where(x => !x.IsDefault))
            .Select(x => new DefaultTimeModel {From = x.From, To = x.To, ScheduleStatus = x.Status})
            .ToArrayAsync(cancellationToken);

        return new() {Valid = true, DefaultTimes = defaultTimes};
    }
}