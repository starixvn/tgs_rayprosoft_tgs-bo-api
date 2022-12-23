using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Language;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LanguageController : BaseContextController
{
    private readonly IMediator _mediator;

    public LanguageController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Description("Create a language")]
    public Task<MessageResponse> CreateLanguageAsync(CreateLanguageRequest request) => _mediator.Send(request);

    [HttpGet("{cultureCode}")]
    [Description("Get a language")]
    public Task<GetLanguageResponse> GetLanguageAsync(string cultureCode) =>
        _mediator.Send(new GetLanguageRequest(cultureCode));

    [HttpGet]
    [Description("Get languages")]
    public Task<PagedResponse<LanguageModel>> GetLanguagesAsync([FromQuery] GetLanguagesRequest request) =>
        _mediator.Send(request);

    [HttpPut("{cultureCode}")]
    [Description("Update a language")]
    public Task<MessageResponse> UpdateLanguageAsync(string cultureCode, UpdateLanguageRequest request)
    {
        request.CultureCode = cultureCode;
        return _mediator.Send(request);
    }
}
