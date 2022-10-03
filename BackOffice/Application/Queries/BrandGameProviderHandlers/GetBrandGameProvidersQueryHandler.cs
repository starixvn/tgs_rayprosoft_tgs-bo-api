using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.BrandGameProviderHandlers;

public class
    GetBrandGameProvidersQueryHandler : Handler<GetBrandGameProvidersRequest,
        PagedResponse<BrandGameProviderModel>>
{
    private readonly IBrandGameProviderService _service;

    public GetBrandGameProvidersQueryHandler(IBrandGameProviderService service, IHttpContextAccessor accessor) :
        base(accessor)
    {
        _service = service;
    }

    public override Task<PagedResponse<BrandGameProviderModel>> Handle(
        GetBrandGameProvidersRequest request,
        CancellationToken cancellationToken)
    {
        var brandGameProviders = _service.GetBrandGameProviderQueries(q => q.Where(request.BrandId, x => x.BrandId == request.BrandId)
            .Where(ResellerId != null, r => r.Brand.Licensee.ResellerId == ResellerId)
            .Where(LicenseeId != null, l => l.Brand.LicenseeId == LicenseeId)
            .Where(BrandId != null, b => b.BrandId == BrandId)
            .Where(request.Keyword, x => x.GameProvider.Name.Contains(request.Keyword)));

        if (request.GameType != null)
        {
            brandGameProviders = brandGameProviders
                .Where(g => g.GameProvider.Games
                    .Select(x => x.Type)
                    .Distinct()
                    .ToArray()
                    .Contains(request.GameType.Value));
        }

        var brandGameProviderModels = brandGameProviders.Select(x => new BrandGameProviderModel
        {
            Id = x.Id,
            Status = x.Status,
            GameProviderId = x.GameProviderId,
            GameProviderName = x.GameProvider.Name,
            From = x.Schedules.FirstOrDefault(s => s.IsDefault).From,
            To = x.Schedules.FirstOrDefault(s => s.IsDefault).To,
            DayOfWeek = x.Schedules.FirstOrDefault(s => s.IsDefault).DayOfWeek,
            ScheduleStatus = x.Schedules.FirstOrDefault(s => s.IsDefault).Status
        });

        var response = brandGameProviderModels.CreatePagedResponseAsync(request);
        return response;
    }
}