using FluentValidation;
using TGS.Services.BackOffice.Application.Models.BetLimit;

namespace TGS.Services.BackOffice.Validation.BetLimit;

public class UpdateBetLimitRequestValidator : AbstractValidator<UpdateBetLimitRequest>
{
    public UpdateBetLimitRequestValidator()
    {
        Include(new BetLimitRequestValidator());
    }
}
