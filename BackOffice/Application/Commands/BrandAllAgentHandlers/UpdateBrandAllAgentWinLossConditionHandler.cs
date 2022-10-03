using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;
using TGS.Services.BackOffice.Infrastructure.DataService;
using Brand = TGS.Services.BackOffice.Application.Resources.Brand;

namespace TGS.Services.BackOffice.Application.Commands.BrandAllAgentHandlers;

public class
    UpdateBrandAllAgentWinLossConditionHandler : Handler<UpdateBrandAllAgentWinLossConditionRequest,
        MessageResponse>
{
    private readonly IBrandService _brandService;
    private readonly IBrandAllAgentGameTypeConditionService _service;

    public UpdateBrandAllAgentWinLossConditionHandler(IBrandService brandService,
        IBrandAllAgentGameTypeConditionService service,
        IHttpContextAccessor accessor) :
        base(accessor)
    {
        _service = service;
        _brandService = brandService;
    }

    public override async Task<MessageResponse> Handle(UpdateBrandAllAgentWinLossConditionRequest request,
        CancellationToken cancellationToken)
    {
        var brand = await _brandService.GetBrandAsync(x => x.Id == request.BrandId);

        if (brand == null)
        {
            return CreateInvalidResponse<MessageResponse>(Brand.NotFound);
        }

        var conditions =
            await _service.GetBrandAllAgentGameTypeConditionQueries().Where(x
                => x.BrandId == request.BrandId && x.AllAgentGameType == request.AllAgentGameType &&
                x.AllAgentRebateType == AllAgentRebateType.WinLoss).ToArrayAsync(cancellationToken);

        foreach (var condition in conditions)
        {
            var rank = request.Conditions.FirstOrDefault(x => x.Rank == condition.Rank);
            if (rank != null)
            {
                condition.ActiveNumber = rank.ActiveNumber;
                condition.NetProfit = rank.NetProfit;
                condition.RebatePercentage = rank.RebatePercentage;
            }
        }

        var deleteConditions = conditions.Select(x => x.Rank).Except(request.Conditions.Select(x => x.Rank));
        var createConditions = request.Conditions.Select(x => x.Rank).Except(conditions.Select(x => x.Rank));

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await _service.UpdateBrandAllAgentGameTypeConditionsAsync(conditions);
        await _service.DeleteBrandAllAgentGameTypeConditionsAsync(conditions
            .Where(x => deleteConditions.Contains(x.Rank)).ToArray());
        await _service.CreateBrandAllAgentGameTypeConditionsAsync(request.Conditions
            .Where(x => createConditions.Contains(x.Rank)).Select(x
                => new BrandAllAgentGameTypeCondition
                {
                    Rank = x.Rank,
                    BrandId = request.BrandId,
                    NetProfit = x.NetProfit,
                    ActiveNumber = x.ActiveNumber,
                    RebatePercentage = x.RebatePercentage,
                    AllAgentGameType = request.AllAgentGameType,
                    AllAgentRebateType = AllAgentRebateType.WinLoss
                }).ToArray());

        scope.Complete();

        return CreateValidResponse<MessageResponse>();
    }
}