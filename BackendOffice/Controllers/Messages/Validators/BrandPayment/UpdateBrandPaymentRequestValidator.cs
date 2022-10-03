using FluentValidation;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Validators.BrandPayment;

public class UpdateBrandPaymentRequestValidator : AbstractValidator<UpdateBrandPaymentRequest>
{
    public UpdateBrandPaymentRequestValidator()
    {
        Include(new BrandPaymentRequestValidator());
    }
}