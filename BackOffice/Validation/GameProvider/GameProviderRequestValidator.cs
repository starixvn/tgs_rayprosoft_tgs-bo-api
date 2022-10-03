using System.Linq;
using FluentValidation;
using TGS.Services.BackOffice.Application.Models.GameProvider;

namespace TGS.Services.BackOffice.Validation.GameProvider;

public class GameProviderRequestValidator : AbstractValidator<GameProviderRequest>
{
    public GameProviderRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage(Application.Resources.GameProvider.CodeRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Application.Resources.GameProvider.NameRequired);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage(Application.Resources.GameProvider.StatusRequired);

        RuleFor(x => x.Currencies)
            .Must(x => x != null && x.Any())
            .WithMessage(Application.Resources.GameProvider.CurrencyRequired);

        RuleForEach(x => x.Currencies)
            .ChildRules(currencies =>
            {
                currencies.RuleFor(x => x.Code).NotEmpty()
                    .WithMessage(Application.Resources.GameProvider.CurrencyRequired);
                currencies.RuleFor(x => x.GameProviderCurrencyCode).NotEmpty()
                    .WithMessage(Application.Resources.GameProvider.CurrencyRequired);
            });

        RuleFor(x => x.Languages)
            .NotEmpty()
            .WithMessage(Application.Resources.GameProvider.LanguageRequired);
    }
}