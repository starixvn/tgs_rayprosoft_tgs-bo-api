using FluentValidation;
using TGS.Services.BackOffice.Application.Models.Language;

namespace TGS.Services.BackOffice.Validation.Language;

public class CreateLanguageRequestValidator : AbstractValidator<CreateLanguageRequest>
{
    public CreateLanguageRequestValidator()
    {
        Include(new LanguageRequestValidator());
    }
}
