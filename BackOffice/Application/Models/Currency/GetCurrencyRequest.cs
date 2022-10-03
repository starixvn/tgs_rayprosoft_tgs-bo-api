using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Currency;

public class GetCurrencyRequest : IRequest<GetCurrencyResponse>
{
    public GetCurrencyRequest(string code)
    {
        Code = code;
    }

    public string Code { get; set; }
}