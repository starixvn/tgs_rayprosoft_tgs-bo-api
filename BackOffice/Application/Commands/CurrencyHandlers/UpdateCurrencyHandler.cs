using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.Currency;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.CurrencyHandlers;

public class UpdateCurrencyHandler(
    ICurrencyService service,
    IHttpContextAccessor accessor) 
    : Handler<UpdateCurrencyRequest, MessageResponse>(accessor)
{
    private readonly ICurrencyService _service = service;

    public override async Task<MessageResponse> Handle(UpdateCurrencyRequest request,
        CancellationToken cancellationToken)
    {
        var currency = await _service.GetCurrencyAsync(q => q.Code == request.Code);

        if (currency == null)
        {
            return CreateInvalidResponse<MessageResponse>(Currency.NotFound);
        }

        await _service.UpdateCurrencyAsync(currency, request);

        return CreateValidResponse<MessageResponse>();
    }
}
