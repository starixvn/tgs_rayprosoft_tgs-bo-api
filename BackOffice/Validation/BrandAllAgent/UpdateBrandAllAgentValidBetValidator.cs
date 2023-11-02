using FluentValidation;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;

namespace TGS.Services.BackOffice.Validation.BrandAllAgent;

public class UpdateBrandAllAgentValidBetValidator : AbstractValidator<UpdateBrandAllAgentValidBetConditionRequest>
{
    public UpdateBrandAllAgentValidBetValidator()
    {
        _ = RuleFor(x => x.AllAgentGameType)
            .IsInEnum()
            .WithMessage(Application.Resources.BrandAllAgent.AllAgentGameTypeInvalid);
    }
}
