using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Currency;

public class GetCurrencyRequest(string code) : IRequest<GetCurrencyResponse>
{
    public string Code { get; set; } = code;
}
