using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

public class UpdateDefaultTimesRequest : IRequest<MessageResponse>
{
    public Guid BrandGameProviderId { get; set; }
    public SwitchStatus ScheduleStatus { get; set; }
    public DefaultTimeModel[] DefaultTimes { get; set; }
}