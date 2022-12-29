using FluentValidation;
using TGS.Services.BackOffice.Application.Models.BrandGame;

namespace TGS.Services.BackOffice.Validation.BrandGame;

public class UpdateBrandGameTagRequestValidator : AbstractValidator<UpdateBrandGameTagRequest>
{
    public UpdateBrandGameTagRequestValidator()
    {
        _ = RuleFor(x => x.GameTag)
            .IsInEnum()
            .WithMessage(Application.Resources.BrandGame.GameTagNotFound);
    }
}
