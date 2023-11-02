using FluentValidation;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;

namespace TGS.Services.BackOffice.Validation.BrandAllAgent;

public class UpdateBrandAllAgentSettlementIntervalValidator : AbstractValidator<UpdateBrandAllAgentSettleIntervalRequest>
{
    public UpdateBrandAllAgentSettlementIntervalValidator()
    {
        _ = RuleFor(x => x.SettlementIntervalType)
            .IsInEnum()
            .WithMessage(Application.Resources.BrandAllAgent.SettlementIntervalInvalid);
    }
}
