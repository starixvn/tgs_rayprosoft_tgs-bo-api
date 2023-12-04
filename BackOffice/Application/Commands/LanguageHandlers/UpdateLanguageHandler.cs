using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Language;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.LanguageHandlers;

public class UpdateLanguageHandler(ILanguageService service, IHttpContextAccessor accessor) : Handler<UpdateLanguageRequest, MessageResponse>(accessor)
{
    private readonly ILanguageService _service = service;

    public override async Task<MessageResponse> Handle(UpdateLanguageRequest request,
        CancellationToken cancellationToken)
    {
        var language = await _service.GetLanguageAsync(q => q.CultureCode == request.CultureCode);

        if (language == null)
        {
            return CreateInvalidResponse<MessageResponse>(Language.NotFound);
        }

        if (await _service.ExistsAsync(x =>
            request.CultureCode != x.CultureCode && x.LanguageCode == request.LanguageCode))
        {
            return CreateInvalidResponse<MessageResponse>(Language.LanguageCodeExisted);
        }

        await _service.UpdateLanguageAsync(language, request);

        return CreateValidResponse<MessageResponse>();
    }
}
