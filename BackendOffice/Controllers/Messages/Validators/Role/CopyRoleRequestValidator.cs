using FluentValidation;
using TGS.Apis.BackendOffice.Controllers.Messages.Roles;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Validators.Role;

public class CopyRoleRequestValidator : AbstractValidator<CopyRoleRequest>
{
    public CopyRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage(Resources.Role.NameRequired)
            .Must(p => p == null || p.Length < 50)
            .WithMessage(Resources.Role.NameLengthLimit);
    }
}