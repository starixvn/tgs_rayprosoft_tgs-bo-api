using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Language;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.LanguageHandlers;

public class CreateLanguageHandler : Handler<CreateLanguageRequest, MessageResponse>
{
    private readonly ILanguageService _service;

    public CreateLanguageHandler(ILanguageService service, IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
    }

    public override async Task<MessageResponse> Handle(CreateLanguageRequest request,
        CancellationToken cancellationToken)
    {
        if (await _service.ExistsAsync(x => x.CultureCode == request.CultureCode))
        {
            return CreateInvalidResponse<MessageResponse>(Language.CultureCodeExisted);
        }

        await _service.CreateLanguageAsync(request);

        return CreateValidResponse<MessageResponse>();
    }
}
