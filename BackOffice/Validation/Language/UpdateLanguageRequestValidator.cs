using FluentValidation;
using TGS.Services.BackOffice.Application.Models.Language;

namespace TGS.Services.BackOffice.Validation.Language;

public class UpdateLanguageRequestValidator : AbstractValidator<UpdateLanguageRequest>
{
    public UpdateLanguageRequestValidator()
    {
        Include(new LanguageRequestValidator());
    }
}
