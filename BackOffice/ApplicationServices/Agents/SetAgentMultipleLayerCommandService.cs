# nullable enable

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.BrandCertificate;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class SetAgentMultipleLayerCommandService(
    IAgentClient agentClient,
    IBrandServiceClient brandServiceClient)
    : IGetService<SetAgentMultipleLayerCommand, SetAgentMultipleLayerResult>
{
    private readonly IAgentClient _agentClient = agentClient ?? throw new ArgumentNullException(nameof(agentClient));

    public async ValueTask<SetAgentMultipleLayerResult> GetAsync(SetAgentMultipleLayerCommand command, CancellationToken cancellationToken = default)
    {
        var operation = Tgs.Agent.Client.Models.Enums.AgentOperation.Info;

        var agent = await agentClient.GetAgentInfoAsync(new GetAgentInfoRequest(command.AgentId), cancellationToken);

        if (agent is null)
            return new SetAgentMultipleLayerResult(false, Application.Resources.Agent.NotFound);

        var parentAgent = await agentClient.GetAgentInfoByUsernameAsync(
            new GetAgentInfoByUsernameRequest(agent.BrandId, command.ParentUsername), cancellationToken);

        if (!string.IsNullOrEmpty(command.ParentUsername))
        {
            if (command.ParentUsername == agent.Username || parentAgent == null)
            {
                return new SetAgentMultipleLayerResult(false, Application.Resources.Agent.ParentAgentNotExists);
            }

            var childAgents = await _agentClient
            .GetChildAgentInfosAsync(new GetChildAgentInfosRequest(command.AgentId), cancellationToken)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

            if (childAgents.Any(a => a.AgentId == parentAgent.Id))
            {
                return new SetAgentMultipleLayerResult(false, Application.Resources.Agent.UnderSubAgentInvalid);
            }
        }

        var brand = await brandServiceClient.GetBrandFullInfoAsync(agent.BrandId, cancellationToken);

        if (brand!.AgentDividendType == AgentDividendType.MultipleLayer)
        {
            foreach (var condition in command.AgentDividendConditions.OrderBy(x => x.Rank))
            {
                var previous = command.AgentDividendConditions.FirstOrDefault(x => x.Rank == condition.Rank - 1);

                if (previous is not null && (condition.NetProfit < previous.NetProfit ||
                    condition.ActiveNumber < previous.ActiveNumber ||
                    condition.DividendPercentage < previous.DividendPercentage))
                {
                    return new SetAgentMultipleLayerResult(false, string.Format(Application.Resources.Agent.RankMustGreaterThanPervious, condition.Rank));
                }
            }
        }

        await agentClient.SetMultipleLayerAgentInfoAsync(new SetMultipleLayerAgentInfoRequest(agent.Id, parentAgent?.Id,
            command.Type, command.AgentDividendConditions.Select(x
                    => new AgentDividendConditionModel(x.Rank, x.ActiveNumber, x.NetProfit, x.DividendPercentage))
                .ToArray(), command.ModifyUsername), cancellationToken);

        if (!string.IsNullOrEmpty(command.ParentUsername) && agent.ParentAgentId != parentAgent?.Id)
        {
            operation = Tgs.Agent.Client.Models.Enums.AgentOperation.Agent;
        }

        await _agentClient
            .AddAgentAuditLogAsync(new AddAgentAuditLogRequest(command.AgentId, operation, command.ModifyUsername), cancellationToken)
            .ConfigureAwait(false);

        return new SetAgentMultipleLayerResult(true, string.Empty);
    }
}
