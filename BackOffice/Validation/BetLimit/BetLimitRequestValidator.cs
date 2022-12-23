using FluentValidation;
using TGS.Services.BackOffice.Application.Models.BetLimit;

namespace TGS.Services.BackOffice.Validation.BetLimit;

public class BetLimitRequestValidator : AbstractValidator<BetLimitRequest>
{
    public BetLimitRequestValidator()
    {
        _ = RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .WithMessage(Application.Resources.BetLimit.CodeRequired);

        _ = RuleFor(x => x.MinBet)
            .GreaterThanOrEqualTo(0)
            .WithMessage(Application.Resources.BetLimit.MinbetGreaterThanZero);

        _ = RuleFor(x => x.MaxBet)
            .GreaterThan(x => x.MinBet)
            .WithMessage(Application.Resources.BetLimit.MaxbetGreaterThanMinbet);
    }
}
