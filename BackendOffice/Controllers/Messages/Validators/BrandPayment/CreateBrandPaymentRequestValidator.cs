using FluentValidation;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Validators.BrandPayment;

public class CreateBrandPaymentRequestValidator : AbstractValidator<CreateBrandPaymentRequest>
{
    public CreateBrandPaymentRequestValidator()
    {
        Include(new BrandPaymentRequestValidator());
    }
}