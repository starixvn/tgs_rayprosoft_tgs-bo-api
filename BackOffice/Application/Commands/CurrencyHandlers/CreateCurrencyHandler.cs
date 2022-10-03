using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Currency;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.CurrencyHandlers;

public class CreateCurrencyHandler : Handler<CreateCurrencyRequest, MessageResponse>
{
    private readonly ICurrencyService _service;

    public CreateCurrencyHandler(ICurrencyService service, IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
    }

    public override async Task<MessageResponse> Handle(CreateCurrencyRequest request,
        CancellationToken cancellationToken)
    {
        if (await _service.ExistsAsync(x => x.Code == request.Code))
        {
            return CreateInvalidResponse<MessageResponse>(Currency.CodeExisted);
        }

        await _service.CreateCurrencyAsync(request);

        return CreateValidResponse<MessageResponse>();
    }
}