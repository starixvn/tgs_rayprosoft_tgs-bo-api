using System.Linq;
using FluentValidation;
using TGS.Services.BackOffice.Application.Models.GameProvider;

namespace TGS.Services.BackOffice.Validation.GameProvider;

public class GameProviderRequestValidator : AbstractValidator<GameProviderRequest>
{
    public GameProviderRequestValidator()
    {
        _ = RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage(Application.Resources.GameProvider.CodeRequired);

        _ = RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Application.Resources.GameProvider.NameRequired);

        _ = RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage(Application.Resources.GameProvider.StatusRequired);

        _ = RuleFor(x => x.Currencies)
            .Must(x => x != null && x.Length != 0)
            .WithMessage(Application.Resources.GameProvider.CurrencyRequired);

        _ = RuleForEach(x => x.Currencies)
            .ChildRules(currencies =>
            {
                _ = currencies.RuleFor(x => x.Code).NotEmpty()
                    .WithMessage(Application.Resources.GameProvider.CurrencyRequired);
                _ = currencies.RuleFor(x => x.GameProviderCurrencyCode).NotEmpty()
                    .WithMessage(Application.Resources.GameProvider.CurrencyRequired);
            });

        _ = RuleFor(x => x.Languages)
            .NotEmpty()
            .WithMessage(Application.Resources.GameProvider.LanguageRequired);
    }
}
