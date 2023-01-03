using FluentValidation;
using TGS.Services.BackOffice.Application.Models.Currency;

namespace TGS.Services.BackOffice.Validation.Currency;

public class CurrencyRequestValidator : AbstractValidator<CurrencyRequest>
{
    public CurrencyRequestValidator()
    {
        _ = RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage(Application.Resources.Currency.CodeRequired);

        _ = RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Application.Resources.Currency.NameRequired);
    }
}
