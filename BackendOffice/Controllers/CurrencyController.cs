using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Services.BackOffice.Application.Models.Currency;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CurrencyController(IMediator mediator) : BaseContextController
{
    [HttpPost]
    [Description("Create a currency")]
    public Task<MessageResponse> CreateCurrencyAsync(CreateCurrencyRequest request) => mediator.Send(request);

    [HttpGet]
    [BOAuthorize(["Merchant.Resellers", "SystemAdmin.Currency", "SystemAdmin.GameProviders", "SystemAdmin.GameProviderRound", "SystemAdmin.BetLimit.Update"])]
    [Description("Get currencies")]
    public Task<PagedResponse<CurrencyModel>> GetCurrenciesAsync([FromQuery] GetCurrenciesRequest request) =>
        mediator.Send(request);

    [HttpGet("{code}")]
    [Description("Get a currency")]
    public Task<GetCurrencyResponse> GetCurrencyAsync(string code) =>
        mediator.Send(new GetCurrencyRequest(code));

    [HttpPut("{code}")]
    [Description("Update a currency")]
    public Task<MessageResponse> UpdateCurrencyAsync(string code, UpdateCurrencyRequest request)
    {
        request.Code = code;
        return mediator.Send(request);
    }
}