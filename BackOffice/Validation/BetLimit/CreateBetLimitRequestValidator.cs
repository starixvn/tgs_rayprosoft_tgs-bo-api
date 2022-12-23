using FluentValidation;
using TGS.Services.BackOffice.Application.Models.BetLimit;

namespace TGS.Services.BackOffice.Validation.BetLimit;

public class CreateBetLimitRequestValidator : AbstractValidator<CreateBetLimitRequest>
{
    public CreateBetLimitRequestValidator()
    {
        Include(new BetLimitRequestValidator());
    }
}
