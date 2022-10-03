using FluentValidation;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Validators.BrandPayment;

public class BrandPaymentRequestValidator : AbstractValidator<BrandPaymentRequest>
{
    public BrandPaymentRequestValidator()
    {
        RuleFor(x => x.ChannelName)
            .NotEmpty()
            .WithMessage(Resources.BrandPayment.ChannelNameRequired);

        RuleFor(x => x.PaymentMethodChannelId)
            .NotEmpty()
            .WithMessage(Resources.BrandPayment.PaymentRequired);

        RuleFor(x => x.TransferLimitTo)
            .GreaterThan(0)
            .WithMessage(Resources.BrandPayment.TransferLimitGreaterZero);
    }
}