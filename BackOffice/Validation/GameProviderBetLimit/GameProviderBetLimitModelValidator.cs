using FluentValidation;
using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

namespace TGS.Services.BackOffice.Validation.GameProviderBetLimit;

public class GameProviderBetLimitModelValidator : AbstractValidator<GameProviderBetLimitModel>
{
    public GameProviderBetLimitModelValidator()
    {
        _ = RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .NotNull()
            .WithMessage(Application.Resources.GameProviderBetLimit.CurrencyRequired);

        _ = RuleFor(x => x.MinBet)
            .GreaterThanOrEqualTo(0)
            .WithMessage(Application.Resources.GameProviderBetLimit.MinbetGreaterThanZero);

        _ = RuleFor(x => x.MaxBet)
            .GreaterThan(x => x.MinBet)
            .WithMessage(Application.Resources.GameProviderBetLimit.MaxbetGreaterThanMinbet);
    }
}
