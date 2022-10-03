using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Currency;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CurrencyController : BaseContextController
{
    private readonly IMediator _mediator;

    public CurrencyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Description("Get currencies")]
    public Task<PagedResponse<CurrencyModel>> GetCurrenciesAsync([FromQuery] GetCurrenciesRequest request) =>
        _mediator.Send(request);

    [HttpGet("{code}")]
    [Description("Get a currency")]
    public Task<GetCurrencyResponse> GetCurrencyAsync(string code) =>
        _mediator.Send(new GetCurrencyRequest(code));

    [HttpPost]
    [Description("Create a currency")]
    public Task<MessageResponse> CreateCurrencyAsync(CreateCurrencyRequest request) => _mediator.Send(request);

    [HttpPut("{code}")]
    [Description("Update a currency")]
    public Task<MessageResponse> UpdateCurrencyAsync(string code, UpdateCurrencyRequest request)
    {
        request.Code = code;
        return _mediator.Send(request);
    }
}