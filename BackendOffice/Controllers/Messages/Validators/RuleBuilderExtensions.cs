using FluentValidation;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Validators;

public static class RuleBuilderExtensions
{
    public static IRuleBuilder<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder,
        int minimumLength = 8)
    {
        var options = ruleBuilder
            .NotEmpty().WithMessage(Resources.User.PasswordRequired)
            .MinimumLength(minimumLength).WithMessage(Resources.User.PasswordLengthAtLeast)
            .Matches("[0-9]").WithMessage(Resources.User.PasswordNeedNumber)
            .Matches("[a-zA-Z]").WithMessage(Resources.User.PasswordNeedCharacter);
        return options;
    }
}