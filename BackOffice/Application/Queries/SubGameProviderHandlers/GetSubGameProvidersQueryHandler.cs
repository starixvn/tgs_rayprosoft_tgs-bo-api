using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.SubGameProvider;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.SubGameProviderHandlers;

public class
    GetSubGameProvidersQueryHandler : Handler<GetSubGameProvidersRequest, PagedResponse<SubGameProviderModel>>
{
    private readonly ISubGameProviderService _service;

    public GetSubGameProvidersQueryHandler(ISubGameProviderService service, IHttpContextAccessor accessor) :
        base(accessor)
    {
        _service = service;
    }

    public override Task<PagedResponse<SubGameProviderModel>> Handle(GetSubGameProvidersRequest request,
        CancellationToken cancellationToken) =>
        _service.GetSubGameProviderQueries(q => q
                .Where(ResellerId,
                    r => r.GameProvider.Resellers.Select(x => x.ResellerId).Contains(ResellerId.Value))
                .Where(LicenseeId,
                    l => l.GameProvider.Licensees.Select(x => x.LicenseeId).Contains(LicenseeId.Value))
                .Where(BrandId, b => b.GameProvider.Brands.Select(x => x.BrandId).Contains(BrandId.Value))
                .Where(g => g.GameProviderId == request.GameProviderId))
            .Select(x => new SubGameProviderModel
            {
                Name = x.Name,
                Code = x.Code,
                Description = x.Description,
                GameProviderId = x.GameProviderId,
                ParentName = x.GameProvider.Name,
                CreatedByUsername = x.CreatedByUsername,
                UpdatedByUsername = x.UpdatedByUsername,
                CreatedOn = x.CreatedOn,
                UpdatedOn = x.UpdatedOn
            }).CreatePagedResponseAsync(request);
}