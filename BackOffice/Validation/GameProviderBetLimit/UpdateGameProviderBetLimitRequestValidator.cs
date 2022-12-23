using FluentValidation;
using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

namespace TGS.Services.BackOffice.Validation.GameProviderBetLimit;

public class UpdateGameProviderBetLimitRequestValidator : AbstractValidator<CreateGameProviderBetLimitRequest>
{
    public UpdateGameProviderBetLimitRequestValidator()
    {
        Include(new GameProviderBetLimitRequestValidator());
    }
}
