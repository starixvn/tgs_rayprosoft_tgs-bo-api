using FluentValidation;
using TGS.Services.BackOffice.Application.Models.BrandGame;

namespace TGS.Services.BackOffice.Validation.BrandGame;

public class UpdateBrandGameRequestValidator : AbstractValidator<UpdateBrandGameRequest>
{
    public UpdateBrandGameRequestValidator()
    {
        RuleFor(x => x.EffectiveBettingPercentage)
            .GreaterThanOrEqualTo(0)
            .WithMessage(Application.Resources.BrandGameProviderSchedule.PercentageLessThan0)
            .LessThanOrEqualTo(100)
            .WithMessage(Application.Resources.BrandGameProviderSchedule.PercentageGreaterThan100);
    }
}