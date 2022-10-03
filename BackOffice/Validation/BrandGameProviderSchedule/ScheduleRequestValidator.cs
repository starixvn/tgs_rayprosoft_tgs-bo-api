using FluentValidation;
using TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

namespace TGS.Services.BackOffice.Validation.BrandGameProviderSchedule;

public class ScheduleRequestValidator : AbstractValidator<BrandGameProviderRequest>
{
    public ScheduleRequestValidator()
    {
        RuleFor(x => x.From)
            .LessThan(x => x.To)
            .WithMessage(Application.Resources.BrandGameProviderSchedule.ToDateGreaterThanFrom);
        RuleFor(x => x.DayOfWeek)
            .IsInEnum()
            .WithMessage(Application.Resources.BrandGameProviderSchedule.DayOfWeekIncorrect);
    }
}