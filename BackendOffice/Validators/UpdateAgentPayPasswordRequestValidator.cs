using FluentValidation;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Infrastructures.Extensions;

namespace TGS.Apis.BackendOffice.Validators;

public class UpdateAgentPayPasswordRequestValidator : AbstractValidator<UpdateAgentPayPasswordRequest>
{
    public UpdateAgentPayPasswordRequestValidator()
    {
        _ = RuleFor(x => x.PayPassword)
            .NotEmpty()
            .Length(6, 20)
            .WithName(x => x
                .WithPropertyExpression(o => o.PayPassword)
                .GetJsonPropertyName(nameof(x.PayPassword)));
    }
}
