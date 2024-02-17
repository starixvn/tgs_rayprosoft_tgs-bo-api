using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Tgs.Agent.Client;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.AgentHandlers;

public class UpdateAgentHandler(
    IAgentService service,
    IHttpContextAccessor accessor) : Handler<UpdateAgentRequest, MessageResponse>(accessor)
{
    public override async Task<MessageResponse> Handle(
        UpdateAgentRequest request,
        CancellationToken cancellationToken)
    {
        var agent = await service.GetAgentAsync(q => q.Id == request.Id);

        if (agent == null)
        {
            return CreateInvalidResponse<MessageResponse>(Agent.NotFound);
        }

        if (request.ParentUsername.IsNotNullOrEmpty())
        {
            var parentAgent = await service.GetAgentAsync(x => x.BrandId == request.BrandId &&
                x.Username == request.ParentUsername);

            if (request.ParentUsername == agent.Username || parentAgent == null)
            {
                return CreateInvalidResponse<MessageResponse>(Agent.ParentAgentNotExists);
            }

            if (parentAgent.Brand.AgentDividendType == AgentDividendType.OneLayer &&
                parentAgent.DividendPercentage < request.DividendPercentage)
            {
                return CreateInvalidResponse<MessageResponse>(Agent
                    .DividendPercentageMustLessThanOrEqualToUpperLayer);
            }

            if (await service.GetAgentQueries()
                    .Where(x => x.Id == parentAgent.Id)
                    .AnyAsync(x => x.Path.MatchesLTxtQuery(agent.Id.ToString("N")), cancellationToken: cancellationToken)
                    .ConfigureAwait(false))
            {
                return CreateInvalidResponse<MessageResponse>(Agent.UnderSubAgentInvalid);
            }
        }

        if (agent.Brand.AgentDividendType == AgentDividendType.OneLayer)
        {
            var childAgentMaxDividendPercentage = await service.GetAgentQueries()
                .Where(x => x.ParentAgentId == agent.Id)
                .MaxAsync(x => (decimal?)x.DividendPercentage, cancellationToken)
                .ConfigureAwait(false) ?? 0M;

            if (request.DividendPercentage < childAgentMaxDividendPercentage)
            {
                return CreateInvalidResponse<MessageResponse>(Agent
                    .DividendPercentageMustGreaterThanOrEqualToLowerLayer);
            }
        }

        if (agent.Brand.AgentDividendType == AgentDividendType.MultipleLayer)
        {
            foreach (var condition in request.AgentDividendConditions.OrderBy(x => x.Rank))
            {
                var previous = request.AgentDividendConditions.FirstOrDefault(x => x.Rank == condition.Rank - 1);

                if (previous is not null && (condition.NetProfit < previous.NetProfit ||
                    condition.ActiveNumber < previous.ActiveNumber ||
                    condition.DividendPercentage < previous.DividendPercentage))
                {
                    return CreateInvalidResponse<MessageResponse>(string.Format(Agent.RankMustGreaterThanPervious,
                        condition.Rank));
                }
            }
        }

        await service.UpdateAgentAsync(agent, request);

        return CreateValidResponse<MessageResponse>();
    }
}
