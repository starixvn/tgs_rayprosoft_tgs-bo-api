using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BrandAgentDividend;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.BrandAgentDividendHandlers;

public class GetBrandAgentDividendConditionsQueryHandler(IBrandAgentDividendConditionService service,
    IHttpContextAccessor accessor) : Handler<GetBrandAgentDividendConditionsRequest,
    PagedResponse<BrandAgentDividendConditionModel>>(accessor)
{
    private readonly IBrandAgentDividendConditionService _service = service;

    public override Task<PagedResponse<BrandAgentDividendConditionModel>> Handle(
        GetBrandAgentDividendConditionsRequest request,
        CancellationToken cancellationToken) =>
        _service.GetBrandAgentDividendConditionQueries().Where(x => x.BrandId == request.BrandId).Select(x
            => new BrandAgentDividendConditionModel
            {
                Rank = x.Rank,
                NetProfit = x.NetProfit,
                ActiveNumber = x.ActiveNumber,
                DividendPercentage = x.DividendPercentage
            }).CreatePagedResponseAsync(new PageableRequest { ShowAll = true });
}
