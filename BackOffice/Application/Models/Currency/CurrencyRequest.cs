namespace TGS.Services.BackOffice.Application.Models.Currency;

public class CurrencyRequest : BaseRequest
{
    public string Code { get; set; }

    public string Name { get; set; }

    public string NativeName { get; set; }

    public string Symbol { get; set; }
}
