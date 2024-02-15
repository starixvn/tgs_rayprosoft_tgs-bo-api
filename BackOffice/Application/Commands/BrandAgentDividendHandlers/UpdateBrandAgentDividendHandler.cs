using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.BrandAgentDividend;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.BrandAgentDividendHandlers;

public class UpdateBrandAgentDividendHandler(
    IBrandAgentDividendConditionService service,
    IBrandService brandService,
    IHttpContextAccessor accessor) 
    : Handler<UpdateBrandAgentDividendConditionRequest, MessageResponse>(accessor)
{
    public override async Task<MessageResponse> Handle(UpdateBrandAgentDividendConditionRequest request,
        CancellationToken cancellationToken)
    {
        if (!await brandService.ExistsAsync(x
            => x.Id == request.BrandId && x.AgentDividendType == AgentDividendType.MultipleLayer))
        {
            return CreateInvalidResponse<MessageResponse>(BrandAgentDividendCondition.AgentDividendTypeInvalid);
        }

        foreach (var condition in request.BrandAgentDividendConditions.OrderBy(x => x.Rank))
        {
            var previous = request.BrandAgentDividendConditions.FirstOrDefault(x => x.Rank == condition.Rank - 1);

            if (previous is not null && (condition.NetProfit < previous.NetProfit ||
                condition.ActiveNumber < previous.ActiveNumber ||
                condition.DividendPercentage < previous.DividendPercentage))
            {
                return CreateInvalidResponse<MessageResponse>(string.Format(Agent.RankMustGreaterThanPervious,
                    condition.Rank));
            }
        }

        var brandAgentDividendConditions = request.BrandAgentDividendConditions.Select(x
            => new Core.DataServices.Tgs.Models.BrandAgentDividendCondition
            {
                BrandId = request.BrandId,
                Rank = x.Rank,
                ActiveNumber = x.ActiveNumber,
                NetProfit = x.NetProfit,
                DividendPercentage = x.DividendPercentage
            }).ToArray();

        await service.UpdateBrandAgentDividendConditionsAsync(request.BrandId, brandAgentDividendConditions);

        return CreateValidResponse<MessageResponse>();
    }
}
