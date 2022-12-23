using FluentValidation;
using TGS.Apis.BackendOffice.Controllers.Messages.Reseller;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Validators.Reseller;

public class CreateResellerRequestValidator : AbstractValidator<CreateResellerRequest>
{
    public CreateResellerRequestValidator()
    {
        Include(new ResellerRequestValidator());
    }
}
