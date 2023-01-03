using FluentValidation;
using TGS.Apis.BackendOffice.Controllers.Messages.Reseller;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Validators.Reseller;

public class UpdateResellerRequestValidator : AbstractValidator<UpdateResellerRequest>
{
    public UpdateResellerRequestValidator()
    {
        Include(new ResellerRequestValidator());
    }
}
