using FluentValidation;
using TGS.Services.BackOffice.Application.Models.Currency;

namespace TGS.Services.BackOffice.Validation.Currency;

public class CreateCurrencyRequestValidator : AbstractValidator<CreateCurrencyRequest>
{
    public CreateCurrencyRequestValidator()
    {
        Include(new CurrencyRequestValidator());
    }
}
