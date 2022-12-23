using FluentValidation;
using TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;

namespace TGS.Services.BackOffice.Validation.BrandGameProviderSchedule;

public class UpdateDefaultTimesRequestValidator : AbstractValidator<UpdateDefaultTimesRequest>
{
    public UpdateDefaultTimesRequestValidator()
    {
        _ = RuleForEach(x => x.DefaultTimes)
            .ChildRules(x =>
            {
                _ = x.RuleFor(m => m.From)
                    .LessThan(m => m.To)
                    .WithMessage(Application.Resources.BrandGameProviderSchedule.ToDateGreaterThanFrom);
            });
    }
}
