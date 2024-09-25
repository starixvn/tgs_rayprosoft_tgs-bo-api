using FluentValidation;
using TGS.Apis.BackendOffice.Controllers.Messages.Users;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Validators.User;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        _ = RuleFor(p => p.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage(Resources.User.NameRequired);

        _ = RuleFor(p => p.Password)
            .NotNull()
            .WithMessage(Resources.User.PasswordRequired)
            .Equal(p => p.ConfirmPassword)
            .WithMessage(Resources.User.PasswordNotMatched)
            .Password();
    }
}
