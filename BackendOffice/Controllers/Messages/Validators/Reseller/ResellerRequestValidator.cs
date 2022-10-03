using FluentValidation;
using TGS.Apis.BackendOffice.Controllers.Messages.Reseller;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Validators.Reseller;

public class ResellerRequestValidator : AbstractValidator<ResellerRequest>
{
    public ResellerRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage(Resources.Reseller.CodeRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Resources.Reseller.NameRequired);

        RuleFor(x => x.GameProviderIds)
            .Must(x => x.Length > 0)
            .WithMessage(Resources.Reseller.GameProviderRequired);

        RuleFor(x => x.CurrencyCodes)
            .Must(x => x.Length > 0)
            .WithMessage(Resources.Reseller.CurrencyRequired);

        RuleFor(x => x.CultureCodes)
            .Must(x => x.Length > 0)
            .WithMessage(Resources.Reseller.CultureRequired);
    }
}