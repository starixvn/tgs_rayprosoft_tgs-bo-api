using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

public class DefaultTimeModel
{
    public DateTimeOffset From { get; set; }

    public SwitchStatus ScheduleStatus { get; set; }

    public DateTimeOffset To { get; set; }
}
