using FluentValidation;
using TGS.Services.BackOffice.Application.Models.Player;

namespace TGS.Services.BackOffice.Validation.Player;

public class UpdatePlayerRequestValidator : AbstractValidator<UpdatePlayerRequest>
{
    public UpdatePlayerRequestValidator()
    {
        RuleFor(p => p.Password)
            .Equal(p => p.ConfirmedPassword).When(p => p.Password != null && p.ConfirmedPassword != null)
            .WithMessage(Application.Resources.Player.PasswordNotMatched)
            .NotEmpty().When(p => p.Password != null).WithMessage(Application.Resources.Player.PasswordRequired)
            .MinimumLength(8).When(p => p.Password != null)
            .WithMessage(Application.Resources.Player.PasswordLengthAtLeast)
            .Matches("[0-9]").When(p => p.Password != null)
            .WithMessage(Application.Resources.Player.PasswordNeedNumber)
            .Matches("[a-zA-Z]").When(p => p.Password != null)
            .WithMessage(Application.Resources.Player.PasswordNeedCharacter);
    }
}