using FluentValidation;
using TGS.Apis.BackendOffice.Controllers.Messages.Users;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Validators.User;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(p => p.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage(Resources.User.NameRequired);
    }
}