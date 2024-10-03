using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Services.BackOffice.Application.Models.Language;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LanguageController(IMediator mediator) : BaseContextController
{
    [HttpPost]
    [Description("Create a language")]
    public Task<MessageResponse> CreateLanguageAsync(CreateLanguageRequest request) => mediator.Send(request);

    [HttpGet("{cultureCode}")]
    [Description("Get a language")]
    public Task<GetLanguageResponse> GetLanguageAsync(string cultureCode) =>
        mediator.Send(new GetLanguageRequest(cultureCode));

    [HttpGet]
    [Description("Get languages")]
    public Task<PagedResponse<LanguageModel>> GetLanguagesAsync([FromQuery] GetLanguagesRequest request) =>
        mediator.Send(request);

    [HttpPut("{cultureCode}")]
    [Description("Update a language")]
    public Task<MessageResponse> UpdateLanguageAsync(string cultureCode, UpdateLanguageRequest request)
    {
        request.CultureCode = cultureCode;
        return mediator.Send(request);
    }
}