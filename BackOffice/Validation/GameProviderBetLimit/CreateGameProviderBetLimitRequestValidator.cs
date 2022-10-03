using FluentValidation;
using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

namespace TGS.Services.BackOffice.Validation.GameProviderBetLimit;

public class CreateGameProviderBetLimitRequestValidator : AbstractValidator<CreateGameProviderBetLimitRequest>
{
    public CreateGameProviderBetLimitRequestValidator()
    {
        Include(new GameProviderBetLimitRequestValidator());
    }
}