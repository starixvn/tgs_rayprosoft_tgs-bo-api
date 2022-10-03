using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Language;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.LanguageHandlers;

public class GetLanguagesQueryHandler : Handler<GetLanguagesRequest, PagedResponse<LanguageModel>>
{
    private readonly ILanguageService _service;

    public GetLanguagesQueryHandler(ILanguageService service, IHttpContextAccessor accessor) :
        base(accessor)
    {
        _service = service;
    }

    public override Task<PagedResponse<LanguageModel>> Handle(GetLanguagesRequest request,
        CancellationToken cancellationToken) =>
        _service.GetLanguageQueries(q => q.Where(ResellerId != null,
                    r => r.ResellerLanguages.Select(x => x.ResellerId).Contains(ResellerId.Value))
                .Where(LicenseeId != null,
                    l => l.LicenseeLanguages.Select(x => x.LicenseeId).Contains(LicenseeId.Value))
                .Where(BrandId != null, b => b.BrandLanguages.Select(x => x.BrandId).Contains(BrandId.Value))
                .Where(request.CultureCode, g => g.CultureCode == request.CultureCode)
                .Where(request.LanguageCode, g => g.LanguageCode == request.LanguageCode))
            .Select(x => new LanguageModel
            {
                CultureCode = x.CultureCode,
                LanguageCode = x.LanguageCode,
                Name = x.Name,
                LocalizedName = x.LocalizedName,
                CreatedByUsername = x.CreatedByUsername,
                UpdatedByUsername = x.UpdatedByUsername,
                UpdatedOn = x.UpdatedOn
            }).CreatePagedResponseAsync(request);
}