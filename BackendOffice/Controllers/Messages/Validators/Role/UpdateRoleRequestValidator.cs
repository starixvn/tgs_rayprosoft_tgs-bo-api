using FluentValidation;
using TGS.Apis.BackendOffice.Controllers.Messages.Roles;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Validators.Role;

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        _ = RuleFor(x => x.Name)
            .NotNull()
            .WithMessage(Resources.Role.NameRequired);

        _ = RuleFor(x => x.Name)
            .Must(p => p == null || p.Length < 50)
            .WithMessage(Resources.Role.NameLengthLimit);
    }
}
