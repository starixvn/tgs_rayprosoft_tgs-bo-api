using FluentValidation;
using TGS.Services.BackOffice.Application.Models.Language;

namespace TGS.Services.BackOffice.Validation.Language;

public class LanguageRequestValidator : AbstractValidator<LanguageRequest>
{
    public LanguageRequestValidator()
    {
        RuleFor(x => x.CultureCode)
            .NotEmpty()
            .WithMessage(Application.Resources.Language.CultureCodeRequired);

        RuleFor(x => x.LanguageCode)
            .NotEmpty()
            .WithMessage(Application.Resources.Language.LanguageCodeRequired);
    }
}