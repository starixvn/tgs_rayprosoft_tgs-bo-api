using FluentValidation;
using TGS.Apis.BackendOffice.Controllers.Messages.Licensee;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Validators.Licensee;

public class UpdateLicenseeRequestValidator : AbstractValidator<UpdateLicenseeRequest>
{
    public UpdateLicenseeRequestValidator()
    {
        Include(new LicenseeRequestValidator());
    }
}
