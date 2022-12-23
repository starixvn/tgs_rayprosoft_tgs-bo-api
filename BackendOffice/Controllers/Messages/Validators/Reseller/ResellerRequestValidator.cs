using FluentValidation;
using TGS.Apis.BackendOffice.Controllers.Messages.Reseller;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Validators.Reseller;

public class ResellerRequestValidator : AbstractValidator<ResellerRequest>
{
    public ResellerRequestValidator()
    {
        _ = RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage(Resources.Reseller.CodeRequired);

        _ = RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Resources.Reseller.NameRequired);

        _ = RuleFor(x => x.GameProviderIds)
            .Must(x => x.Length > 0)
            .WithMessage(Resources.Reseller.GameProviderRequired);

        _ = RuleFor(x => x.CurrencyCodes)
            .Must(x => x.Length > 0)
            .WithMessage(Resources.Reseller.CurrencyRequired);

        _ = RuleFor(x => x.CultureCodes)
            .Must(x => x.Length > 0)
            .WithMessage(Resources.Reseller.CultureRequired);
    }
}
