using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.Currency;

public class GetCurrencyResponse : MessageResponse
{
    public CurrencyModel Currency { get; set; }
}
