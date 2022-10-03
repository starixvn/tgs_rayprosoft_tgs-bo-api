using FluentValidation;
using TGS.Services.BackOffice.Application.Models.Currency;

namespace TGS.Services.BackOffice.Validation.Currency;

public class UpdateCurrencyRequestValidator : AbstractValidator<UpdateCurrencyRequest>
{
    public UpdateCurrencyRequestValidator()
    {
        Include(new CurrencyRequestValidator());
    }
}