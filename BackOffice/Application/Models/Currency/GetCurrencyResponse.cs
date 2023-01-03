using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Currency;

public class GetCurrencyResponse : MessageResponse
{
    public CurrencyModel Currency { get; set; }
}
