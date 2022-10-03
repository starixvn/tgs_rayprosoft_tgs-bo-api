using FluentValidation;
using TGS.Apis.BackendOffice.Controllers.Messages.Licensee;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Validators.Licensee;

public class LicenseeRequestValidator : AbstractValidator<LicenseeRequest>
{
    public LicenseeRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage(Resources.Licensee.CodeRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Resources.Licensee.NameRequired);

        RuleFor(x => x.GameProviderIds)
            .Must(x => x.Length > 0)
            .WithMessage(Resources.Licensee.GameProviderRequired);

        RuleFor(x => x.CurrencyCodes)
            .Must(x => x.Length > 0)
            .WithMessage(Resources.Licensee.CurrencyRequired);

        RuleFor(x => x.CultureCodes)
            .Must(x => x.Length > 0)
            .WithMessage(Resources.Licensee.CultureRequired);
    }
}