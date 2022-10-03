using FluentValidation;
using TGS.Apis.BackendOffice.Controllers.Messages.Users;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Validators.User;

public class UpdatePasswordRequestValidator : AbstractValidator<UpdatePasswordRequest>
{
    public UpdatePasswordRequestValidator()
    {
        RuleFor(p => p.Password)
            .NotNull()
            .WithMessage(Resources.User.PasswordRequired)
            .Equal(p => p.ConfirmPassword)
            .WithMessage(Resources.User.PasswordNotMatched)
            .Password();
    }
}