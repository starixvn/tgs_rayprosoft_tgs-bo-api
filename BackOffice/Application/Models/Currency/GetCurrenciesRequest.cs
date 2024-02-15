using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.Currency;

public class GetCurrenciesRequest : PageableRequest, IRequest<PagedResponse<CurrencyModel>>
{
    public string Code { get; set; }

    public string Name { get; set; }
}
