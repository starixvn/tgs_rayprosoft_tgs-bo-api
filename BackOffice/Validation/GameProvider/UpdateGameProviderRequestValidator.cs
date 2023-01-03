using FluentValidation;
using TGS.Services.BackOffice.Application.Models.GameProvider;

namespace TGS.Services.BackOffice.Validation.GameProvider;

public class UpdateGameProviderRequestValidator : AbstractValidator<UpdateGameProviderRequest>
{
    public UpdateGameProviderRequestValidator()
    {
        Include(new GameProviderRequestValidator());
    }
}
