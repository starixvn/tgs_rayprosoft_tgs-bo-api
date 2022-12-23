using FluentValidation;
using TGS.Apis.BackendOffice.Controllers.Messages.Licensee;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Validators.Licensee;

public class CreateLicenseeRequestValidator : AbstractValidator<CreateLicenseeRequest>
{
    public CreateLicenseeRequestValidator()
    {
        Include(new LicenseeRequestValidator());
    }
}
