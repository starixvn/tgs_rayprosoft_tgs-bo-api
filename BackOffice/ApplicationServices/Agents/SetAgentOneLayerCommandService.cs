# nullable enable

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.BrandCertificate;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class SetAgentOneLayerCommandService(
    IAgentClient agentClient,
    IBrandServiceClient brandServiceClient) : IGetService<SetAgentOneLayerCommand, SetAgentOneLayerResult>
{
    public async ValueTask<SetAgentOneLayerResult> GetAsync(SetAgentOneLayerCommand command,
        CancellationToken cancellationToken = default)
    {
        var operation = Tgs.Agent.Client.Models.Enums.AgentOperation.Info;

        var agent = await agentClient.GetAgentInfoAsync(new GetAgentInfoRequest(command.AgentId), cancellationToken);

        if (agent is null)
            return new SetAgentOneLayerResult(false, Application.Resources.Agent.NotFound);

        var parentAgent = await agentClient.GetAgentInfoByUsernameAsync(
            new GetAgentInfoByUsernameRequest(agent.BrandId, command.ParentUsername), cancellationToken);

        var brand = await brandServiceClient.GetBrandFullInfoAsync(agent.BrandId, cancellationToken);

        var childAgents = await agentClient
            .GetChildAgentInfosAsync(new GetChildAgentInfosRequest(command.AgentId), cancellationToken)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!string.IsNullOrEmpty(command.ParentUsername))
        {
            if (command.ParentUsername == agent.Username || parentAgent == null)
            {
                return new SetAgentOneLayerResult(false, Application.Resources.Agent.ParentAgentNotExists);
            }

            if (brand!.AgentDividendType == AgentDividendType.OneLayer
                && parentAgent.DividendPercentage < command.DividendPercentage)
            {
                return new SetAgentOneLayerResult(false, Application.Resources.Agent.DividendPercentageMustLessThanOrEqualToUpperLayer);
            }

            if (childAgents.Any(a => a.AgentId == parentAgent.Id))
            {
                return new SetAgentOneLayerResult(false, Application.Resources.Agent.UnderSubAgentInvalid);
            }
        }

        if (brand!.AgentDividendType == AgentDividendType.OneLayer)
        {
            var childAgentMaxDividendPercentage = childAgents
                .Max(x => (decimal?)x.DividendPercentage) ?? 0M;

            if (command.DividendPercentage < childAgentMaxDividendPercentage)
                return new SetAgentOneLayerResult(false, Application.Resources.Agent.DividendPercentageMustGreaterThanOrEqualToLowerLayer);
        }

        await agentClient.SetOneLayerAgentInfoAsync(new SetOneLayerAgentInfoRequest(agent.Id, parentAgent?.Id,
            command.DividendPercentage, command.Type, command.ModifyUsername), cancellationToken);

        if (!string.IsNullOrEmpty(command.ParentUsername) && agent.ParentAgentId != parentAgent?.Id)
        {
            operation = Tgs.Agent.Client.Models.Enums.AgentOperation.Agent;
        }

        await agentClient
            .AddAgentAuditLogAsync(new AddAgentAuditLogRequest(command.AgentId, operation, command.ModifyUsername),
                cancellationToken)
            .ConfigureAwait(false);
        return new SetAgentOneLayerResult(true, string.Empty);
    }
}
