using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Currency;

public class GetCurrencyRequest : IRequest<GetCurrencyResponse>
{
    public string Code { get; set; }

    public GetCurrencyRequest(string code)
    {
        Code = code;
    }
}
