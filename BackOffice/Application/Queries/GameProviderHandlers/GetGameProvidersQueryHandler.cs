using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TGS.Services.BackOffice.Application.Models.GameProvider;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.GameProviderHandlers;

public class GetGameProvidersQueryHandler(
    IGameProviderService service,
    IHttpContextAccessor accessor)
    : Handler<GetGameProvidersRequest, PagedResponse<GameProviderBaseModel>>(accessor)
{
    public override Task<PagedResponse<GameProviderBaseModel>> Handle(
        GetGameProvidersRequest request,
        CancellationToken cancellationToken)
        => service.GetGameProviderQueries(q => q
            .Where(ResellerId, r => r.Resellers.Select(x => x.ResellerId).Contains(ResellerId.Value))
            .Where(LicenseeId, l => l.Licensees.Select(x => x.LicenseeId).Contains(LicenseeId.Value))
            .Where(BrandId, b => b.Brands.Select(x => x.BrandId).Contains(BrandId.Value))
            .Where(request.Code, g => EF.Functions.Like(g.Code.ToLower(), $"%{request.Code.ToLower()}%"))
            .Where(request.Name, g => EF.Functions.Like(g.Name.ToLower(), $"%{request.Name.ToLower()}%")))
            .Where(g => request.Status == null || g.Status == request.Status)
            .Select(x => new GameProviderBaseModel
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                Status = x.Status,
                Description = x.Description,
                UpdatedByUsername = x.UpdatedByUsername,
                UpdatedOn = x.UpdatedOn
            })
            .CreatePagedResponseAsync(request);
}
