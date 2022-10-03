using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.BrandHandlers;

public class GetBrandsQueryHandler : Handler<GetBrandsRequest, PagedResponse<BrandModel>>
{
    private readonly IBrandService _service;

    public GetBrandsQueryHandler(IBrandService service, IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
    }

    public override Task<PagedResponse<BrandModel>> Handle(GetBrandsRequest request,
        CancellationToken cancellationToken) =>
        _service.GetBrandQueries(q => q.Where(request.ResellerId, x => x.Licensee.ResellerId == request.ResellerId)
                .Where(request.LicenseeId, x => x.LicenseeId == request.LicenseeId)
                .Where(request.BrandId, x => x.Id == request.BrandId)
                .Where(ResellerId, r => r.Licensee.ResellerId == ResellerId)
                .Where(LicenseeId, l => l.LicenseeId == LicenseeId)
                .Where(BrandId, l => l.Id == BrandId)
                .Where(request.Keyword, x => EF.Functions.ILike(x.Code, $"%{request.Keyword}%") ||
                    EF.Functions.ILike(x.Name, $"%{request.Keyword}%") ||
                    EF.Functions.ILike(x.Description, $"%{request.Keyword}%") ||
                    EF.Functions.ILike(x.OperationalEmail, $"%{request.Keyword}%")))
            .Select(x => new BrandModel
            {
                Id = x.Id,
                LicenseeId = x.LicenseeId,
                LicenseeName = x.Licensee.Name,
                Name = x.Name,
                Code = x.Code,
                Status = x.Status,
                Description = x.Description,
                OperationalEmail = x.OperationalEmail,
                CreatedOn = x.CreatedOn,
                UpdatedOn = x.UpdatedOn
            }).CreatePagedResponseAsync(request);
}