using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Currency;

public class GetCurrenciesRequest : PageableRequest, IRequest<PagedResponse<CurrencyModel>>
{
    public string Code { get; set; }
    public string Name { get; set; }
}