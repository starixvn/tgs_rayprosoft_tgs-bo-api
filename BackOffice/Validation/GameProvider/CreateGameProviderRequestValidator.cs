using FluentValidation;
using TGS.Services.BackOffice.Application.Models.GameProvider;

namespace TGS.Services.BackOffice.Validation.GameProvider;

public class CreateGameProviderRequestValidator : AbstractValidator<CreateGameProviderRequest>
{
    public CreateGameProviderRequestValidator()
    {
        Include(new GameProviderRequestValidator());
    }
}
