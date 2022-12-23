using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.BrandAllAgentHandlers;

public class GetBrandAllAgentWinLossSettingQueryHandler : Handler<GetBrandAllAgentWinLossSettingRequest,
    WinLossConditionModel[]>
{
    private readonly IBrandAllAgentGameTypeConditionService _brandAllAgentGameTypeConditionService;

    public GetBrandAllAgentWinLossSettingQueryHandler(
        IBrandAllAgentGameTypeConditionService brandAllAgentGameTypeConditionService,
        IHttpContextAccessor accessor) : base(accessor)
        => _brandAllAgentGameTypeConditionService = brandAllAgentGameTypeConditionService;

    public override Task<WinLossConditionModel[]> Handle(
        GetBrandAllAgentWinLossSettingRequest request,
        CancellationToken cancellationToken)
        => _brandAllAgentGameTypeConditionService.GetBrandAllAgentGameTypeConditionQueries()
            .Where(x => x.BrandId == request.BrandId && x.AllAgentGameType == request.AllAgentGameType &&
                x.AllAgentRebateType == AllAgentRebateType.WinLoss).Select(x => new WinLossConditionModel
                {
                    Rank = x.Rank,
                    ActiveNumber = x.ActiveNumber,
                    NetProfit = x.NetProfit,
                    RebatePercentage = x.RebatePercentage
                })
            .ToArrayAsync(cancellationToken);
}
