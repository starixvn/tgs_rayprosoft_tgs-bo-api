using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.GameProvider;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.GameProviderHandlers;

public class GetGameProvidersQueryHandler(IGameProviderService service, IHttpContextAccessor accessor)
    : Handler<GetGameProvidersRequest, PagedResponse<GameProviderBaseModel>>(accessor)
{
    private readonly IGameProviderService _service = service;

    public override Task<PagedResponse<GameProviderBaseModel>> Handle(
        GetGameProvidersRequest request,
        CancellationToken cancellationToken)
        => _service.GetGameProviderQueries(q => q
                .Where(ResellerId, r => r.Resellers.Select(x => x.ResellerId).Contains(ResellerId.Value))
                .Where(LicenseeId, l => l.Licensees.Select(x => x.LicenseeId).Contains(LicenseeId.Value))
                .Where(BrandId, b => b.Brands.Select(x => x.BrandId).Contains(BrandId.Value))
                .Where(request.Code, g => g.Code == request.Code)
                .Where(request.Name, g => g.Name == request.Name))
            .Select(x => new GameProviderBaseModel
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                Status = x.Status,
                Description = x.Description,
                UpdatedByUsername = x.UpdatedByUsername,
                UpdatedOn = x.UpdatedOn
            }).CreatePagedResponseAsync(request);
}
