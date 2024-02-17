using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

public class UpdateDefaultTimesRequest : IRequest<MessageResponse>
{
    public Guid BrandGameProviderId { get; set; }

    public DefaultTimeModel[] DefaultTimes { get; set; }

    public SwitchStatus ScheduleStatus { get; set; }
}
