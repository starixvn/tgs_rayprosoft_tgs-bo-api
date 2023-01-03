using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.BrandGameProviderHandlers;

public class UpdateBrandGameProviderHandler : Handler<UpdateBrandGameProviderRequest, MessageResponse>
{
    private readonly IBrandGameProviderScheduleService _scheduleService;
    private readonly IBrandGameProviderService _service;

    public UpdateBrandGameProviderHandler(IBrandGameProviderService service,
        IBrandGameProviderScheduleService scheduleService,
        IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
        _scheduleService = scheduleService;
    }

    public override async Task<MessageResponse> Handle(UpdateBrandGameProviderRequest request,
        CancellationToken cancellationToken)
    {
        var brand = await _service.GetBrandGameProviderAsync(q => q.Id == request.Id);

        if (brand == null)
        {
            return CreateInvalidResponse<MessageResponse>(BrandGameProvider.NotFound);
        }

        var schedule = brand.Schedules.FirstOrDefault(x => x.IsDefault);

        if (schedule == null)
        {
            return CreateInvalidResponse<MessageResponse>(BrandGameProviderSchedule.NotFound);
        }

        schedule.From = request.From;
        schedule.To = request.To;
        schedule.DayOfWeek = request.DayOfWeek;
        schedule.Status = request.ScheduleStatus;

        await _scheduleService.UpdateBrandGameProviderScheduleAsync(schedule);

        return CreateValidResponse<MessageResponse>();
    }
}
