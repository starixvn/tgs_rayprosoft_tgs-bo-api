using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.Language;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.LanguageHandlers;

public class GetLanguageQueryHandler : Handler<GetLanguageRequest,
    GetLanguageResponse>
{
    private readonly ILanguageService _service;

    public GetLanguageQueryHandler(ILanguageService service, IHttpContextAccessor accessor)
        : base(accessor)
    {
        _service = service;
    }

    public override async Task<GetLanguageResponse> Handle(
        GetLanguageRequest request,
        CancellationToken cancellationToken)
    {
        var language = await _service.GetLanguageAsync(x => x.CultureCode == request.CultureCode);
        return language == null
            ? CreateInvalidResponse<GetLanguageResponse>(Language.NotFound)
            : new GetLanguageResponse
            {
                Valid = true,
                Language = new LanguageModel
                {
                    CultureCode = language.CultureCode,
                    LanguageCode = language.LanguageCode,
                    Name = language.Name,
                    LocalizedName = language.LocalizedName,
                    CreatedByUsername = language.CreatedByUsername,
                    CreatedOn = language.CreatedOn,
                    UpdatedByUsername = language.UpdatedByUsername,
                    UpdatedOn = language.UpdatedOn
                }
            };
    }
}
