using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tgs.Agent.Dividend.Client;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BrandAgentDividend;

namespace TGS.Services.BackOffice.Application.Queries.BrandAgentDividendHandlers;

public class GetBrandAgentDividendConditionsQueryHandler(
    IBrandContractClient brandContractClient,
    IHttpContextAccessor accessor) : Handler<GetBrandAgentDividendConditionsRequest,
    PagedResponse<BrandAgentDividendConditionModel>>(accessor)
{
    public override async Task<PagedResponse<BrandAgentDividendConditionModel>> Handle(
        GetBrandAgentDividendConditionsRequest request,
        CancellationToken cancellationToken)
    {
        var brandContracts = await brandContractClient.GetBrandContractAsync(request.BrandId, cancellationToken).FirstAsync(cancellationToken);

        return await brandContracts.Conditions.Select(x
            => new BrandAgentDividendConditionModel
            {
                Rank = x.Rank,
                NetProfit = x.NetProfit,
                ActiveNumber = x.ActiveNumber,
                DividendPercentage = x.DividendPercentage
            }).CreatePagedResponseAsync(brandContracts.Conditions.Count());
    }
}
