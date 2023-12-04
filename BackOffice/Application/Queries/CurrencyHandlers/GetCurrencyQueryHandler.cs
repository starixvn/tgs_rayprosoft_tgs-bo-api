using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.Currency;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.CurrencyHandlers;

public class GetCurrencyQueryHandler(ICurrencyService service, IHttpContextAccessor accessor) : Handler<GetCurrencyRequest,
    GetCurrencyResponse>(accessor)
{
    private readonly ICurrencyService _service = service;

    public override async Task<GetCurrencyResponse> Handle(
        GetCurrencyRequest request,
        CancellationToken cancellationToken)
    {
        var currency = await _service.GetCurrencyAsync(x => x.Code == request.Code);
        return currency == null
            ? CreateInvalidResponse<GetCurrencyResponse>(Currency.NotFound)
            : new GetCurrencyResponse
            {
                Valid = true,
                Currency = new CurrencyModel
                {
                    Code = currency.Code,
                    Name = currency.Name,
                    NativeName = currency.NativeName,
                    Symbol = currency.Symbol,
                    CreatedByUsername = currency.CreatedByUsername,
                    CreatedOn = currency.CreatedOn,
                    UpdatedByUsername = currency.UpdatedByUsername,
                    UpdatedOn = currency.UpdatedOn
                }
            };
    }
}
