using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

public class BrandGameProviderModel
{
    public Guid Id { get; set; }
    public Status Status { get; set; }
    public Guid GameProviderId { get; set; }
    public string GameProviderName { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public SwitchStatus ScheduleStatus { get; set; }
}