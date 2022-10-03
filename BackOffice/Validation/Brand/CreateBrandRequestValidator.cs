using System.Linq;
using FluentValidation;
using TGS.Services.BackOffice.Application.Models.Brand;

namespace TGS.Services.BackOffice.Validation.Brand;

public class CreateBrandRequestValidator : AbstractValidator<CreateBrandRequest>
{
    public CreateBrandRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage(Application.Resources.Brand.CodeRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Application.Resources.Brand.NameRequired);

        RuleFor(x => x.GameProviders)
            .NotEmpty()
            .WithMessage(Application.Resources.Brand.GameProviderRequired);

        RuleFor(x => x.CurrencyCodes)
            .NotEmpty()
            .WithMessage(Application.Resources.Brand.CurrencyRequired);

        RuleFor(x => x.SettlementIntervalType)
            .IsInEnum()
            .WithMessage(Application.Resources.Brand.SettlementIntervalInvalid);

        RuleForEach(x => x.CurrencyCodes)
            .Must((m, n) => m.BetLimits.FirstOrDefault(x => x.CurrencyCode == n && x.IsDefault) != null)
            .WithMessage(Application.Resources.Brand.BetLimitDefaultRequired);

        RuleFor(x => x.Languages)
            .NotEmpty()
            .WithMessage(Application.Resources.Brand.CultureRequired);
    }
}