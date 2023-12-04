using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.PlatformCredit;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.PlatformCreditHandlers;

public class GetPlatformCreditsQueryHandler(IPlatformCreditService service, IHttpContextAccessor accessor) : Handler<GetPlatformCreditsRequest, PagedResponse<PlatformCreditModel>>(accessor)
{
    private readonly IPlatformCreditService _service = service;

    public override Task<PagedResponse<PlatformCreditModel>> Handle(GetPlatformCreditsRequest request,
        CancellationToken cancellationToken) =>
        _service.GetPlatformCreditQueries(q => q
                .Where(request.BrandId, x => x.BrandId == request.BrandId)
                .Where(request.LicenseeName, x => EF.Functions.ILike(x.Brand.Licensee.Name, $"%{request.Keyword}%"))
                .Where(request.Keyword, x => EF.Functions.ILike(x.Brand.Code, $"%{request.Keyword}%") ||
                    EF.Functions.ILike(x.Brand.Name, $"%{request.Keyword}%")))
            .Select(x => new PlatformCreditModel
            {
                Id = x.Id,
                LicenseeName = x.Brand.Licensee.Name,
                BrandName = x.Brand.Name,
                BrandCode = x.Brand.Code,
                Credit = x.Credit,
                Balance = x.Balance,
                CreatedOn = x.CreatedOn,
                UpdatedOn = x.UpdatedOn
            }).CreatePagedResponseAsync(request);
}
