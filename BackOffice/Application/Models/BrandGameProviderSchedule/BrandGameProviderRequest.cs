using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

public class BrandGameProviderRequest : BaseRequest
{
    public DayOfWeek DayOfWeek { get; set; }

    public DateTimeOffset From { get; set; }

    public Guid GameProviderId { get; set; }

    public Guid Id { get; set; }

    public SwitchStatus ScheduleStatus { get; set; }

    public DateTimeOffset To { get; set; }
}
