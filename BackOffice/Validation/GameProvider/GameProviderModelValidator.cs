using FluentValidation;
using TGS.Services.BackOffice.Application.Models.GameProvider;

namespace TGS.Services.BackOffice.Validation.GameProvider;

public class GameProviderModelValidator : AbstractValidator<GameProviderModel>
{
    public GameProviderModelValidator()
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
            .NotEmpty()
            .WithMessage(Application.Resources.GameProvider.CurrencyRequired);

        RuleFor(x => x.Languages)
            .NotEmpty()
            .WithMessage(Application.Resources.GameProvider.LanguageRequired);
    }
}