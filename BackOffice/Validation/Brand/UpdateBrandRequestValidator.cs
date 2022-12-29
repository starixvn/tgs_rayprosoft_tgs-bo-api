using System.Linq;
using FluentValidation;
using TGS.Services.BackOffice.Application.Models.Brand;

namespace TGS.Services.BackOffice.Validation.Brand;

public class UpdateBrandRequestValidator : AbstractValidator<UpdateBrandRequest>
{
    public UpdateBrandRequestValidator()
    {
        _ = RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage(Application.Resources.Brand.CodeRequired);

        _ = RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Application.Resources.Brand.NameRequired);

        _ = RuleFor(x => x.GameProviders)
            .NotEmpty()
            .WithMessage(Application.Resources.Brand.GameProviderRequired);

        _ = RuleFor(x => x.CurrencyCodes)
            .NotEmpty()
            .WithMessage(Application.Resources.Brand.CurrencyRequired);

        _ = RuleFor(x => x.SettlementIntervalType)
            .IsInEnum()
            .WithMessage(Application.Resources.Brand.SettlementIntervalInvalid);

        _ = RuleForEach(x => x.CurrencyCodes)
            .Must((m, n) => m.BetLimits.FirstOrDefault(x => x.CurrencyCode == n && x.IsDefault) != null)
            .WithMessage(Application.Resources.Brand.BetLimitDefaultRequired);

        _ = RuleFor(x => x.Languages)
            .NotEmpty()
            .WithMessage(Application.Resources.Brand.CultureRequired);
    }
}
