using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.BrandGameProviderHandlers;

public class UpdateBrandGameProviderDefaultTimesHandler(IBrandGameProviderScheduleService service,
    IHttpContextAccessor accessor) : Handler<UpdateDefaultTimesRequest, MessageResponse>(accessor)
{
    private readonly IBrandGameProviderScheduleService _service = service;

    public override async Task<MessageResponse> Handle(UpdateDefaultTimesRequest request,
        CancellationToken cancellationToken)
    {
        var schedules = await _service.GetBrandGameProviderScheduleQueries(q =>
                q.Where(x => x.BrandGameProviderId == request.BrandGameProviderId && !x.IsDefault))
            .ToArrayAsync(cancellationToken);

        await _service.UpdateBrandGameProviderDefaultTimesAsync(schedules, request);

        return CreateValidResponse<MessageResponse>();
    }
}
