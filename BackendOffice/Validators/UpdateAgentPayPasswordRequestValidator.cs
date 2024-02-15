using FluentValidation;
using TGS.Apis.BackendOffice.ViewModels;

namespace TGS.Apis.BackendOffice.Validators;

public class UpdateAgentPayPasswordRequestValidator : AbstractValidator<UpdateAgentPayPasswordRequest>
{
    public UpdateAgentPayPasswordRequestValidator()
    {
        _ = RuleFor(x => x.PayPassword)
            .NotEmpty()
            .Length(6, 20)
            .WithName("payPassword");
    }
}
