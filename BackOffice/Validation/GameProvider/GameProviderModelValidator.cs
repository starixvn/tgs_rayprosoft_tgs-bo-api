using FluentValidation;
using TGS.Services.BackOffice.Application.Models.GameProvider;

namespace TGS.Services.BackOffice.Validation.GameProvider;

public class GameProviderModelValidator : AbstractValidator<GameProviderModel>
{
    public GameProviderModelValidator()
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
            .NotEmpty()
            .WithMessage(Application.Resources.GameProvider.CurrencyRequired);

        _ = RuleFor(x => x.Languages)
            .NotEmpty()
            .WithMessage(Application.Resources.GameProvider.LanguageRequired);
    }
}
