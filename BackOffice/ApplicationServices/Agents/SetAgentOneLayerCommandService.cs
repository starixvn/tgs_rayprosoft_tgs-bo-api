# nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using NATS.Client.Service;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class SetAgentOneLayerCommandService : IGetService<SetAgentOneLayerCommand, SetAgentOneLayerResult>
{
    private readonly IAgentClient _agentClient;
    private readonly ITgsRepository _repository;

    public SetAgentOneLayerCommandService(
        IAgentClient agentClient,
        ITgsRepository repository)
    {
        _agentClient = agentClient ?? throw new ArgumentNullException(nameof(agentClient));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async ValueTask<SetAgentOneLayerResult> GetAsync(SetAgentOneLayerCommand command, CancellationToken cancellationToken = default)
    {
        var operation = Tgs.Agent.Client.Models.Enums.AgentOperation.Info;

        var agent = await _repository.Agents.Where(x => x.Id == command.AgentId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (agent is null)
            return new SetAgentOneLayerResult(false, Application.Resources.Agent.NotFound);

        var parentAgent = await _repository.Agents.AsQueryable()
            .FirstOrDefaultAsync(x => x.BrandId == agent.BrandId && x.Username == command.ParentUsername, cancellationToken)
            .ConfigureAwait(false);

        if (!string.IsNullOrEmpty(command.ParentUsername))
        {
            if (command.ParentUsername == agent.Username || parentAgent == null)
            {
                return new SetAgentOneLayerResult(false, Application.Resources.Agent.ParentAgentNotExists);
            }

            if (parentAgent.Brand.AgentDividendType == AgentDividendType.OneLayer
                && parentAgent.DividendPercentage < command.DividendPercentage)
            {
                return new SetAgentOneLayerResult(false, Application.Resources.Agent.DividendPercentageMustLessThanOrEqualToUpperLayer);
            }

            if (await _repository.Agents
                    .Where(x => x.Id == parentAgent.Id)
                    .AnyAsync(x => x.Path.MatchesLTxtQuery(agent.Id.ToString("N")), cancellationToken: cancellationToken)
                    .ConfigureAwait(false))
            {
                return new SetAgentOneLayerResult(false, Application.Resources.Agent.UnderSubAgentInvalid);
            }
        }

        if (agent.Brand.AgentDividendType == AgentDividendType.OneLayer)
        {
            var childAgentMaxDividendPercentage = await _repository.Agents
                .Where(x => x.ParentAgentId == agent.Id)
                .MaxAsync(x => (decimal?)x.DividendPercentage, cancellationToken)
                .ConfigureAwait(false) ?? 0M;

            if (command.DividendPercentage < childAgentMaxDividendPercentage)
                return new SetAgentOneLayerResult(false, Application.Resources.Agent.DividendPercentageMustGreaterThanOrEqualToLowerLayer);
        }

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        if (!string.IsNullOrEmpty(command.ParentUsername) && agent.ParentAgentId != parentAgent?.Id)
        {
            operation = Tgs.Agent.Client.Models.Enums.AgentOperation.Agent;

            var agents = await _repository.Agents.AsQueryable()
                .Where(x => x.BrandId == agent.BrandId)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            var updateAgents = GetChildAgents(command.AgentId, agents);

            _repository.Agents.UpdateRange(updateAgents);
            _ = await _repository.SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

            var nLevel = await _repository.Agents
                .Where(x => x.Id == command.AgentId)
                .Select(x => x.Path.NLevel)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            _ = await UpdateSubAgentsPathAsync(agent, nLevel, parentAgent!)
                .ConfigureAwait(false);
        }

        agent.Path =
            operation == Tgs.Agent.Client.Models.Enums.AgentOperation.Agent
            && !string.IsNullOrEmpty(command.ParentUsername)
            && agent.ParentAgentId != parentAgent?.Id
                ? new LTree($"{parentAgent!.Path}.{agent.Id:N}")
                : agent.Path;

        agent.ParentAgentId = parentAgent?.Id;

        agent.LevelType = string.IsNullOrEmpty(command.ParentUsername)
            ? AgentLevelType.Chief
            : AgentLevelType.Agent;

        agent.Type = (AgentType)command.Type;
        agent.DividendPercentage = operation == Tgs.Agent.Client.Models.Enums.AgentOperation.Agent
            ? -1
            : command.DividendPercentage;

        agent.AgentStatus = operation == Tgs.Agent.Client.Models.Enums.AgentOperation.Agent
            ? AgentConfigStatus.WaitForConfigurePercentage
            : AgentConfigStatus.DoneForConfigurePercentage;

        agent.UpdatedByUsername = command.ModifyUsername;
        agent.UpdatedOn = DateTimeOffset.Now;

        _ = _repository.Agents.Update(agent);
        _ = await _repository
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        scope.Complete();

        await _agentClient
            .AddAgentAuditLogAsync(new AddAgentAuditLogRequest(command.AgentId, operation, command.ModifyUsername), cancellationToken)
            .ConfigureAwait(false);
        return new SetAgentOneLayerResult(true, string.Empty);
    }

    private static IEnumerable<Agent> GetChildAgents(Guid parentAgentId, Agent[] agents)
    {
        var childAgents = agents.Where(x => x.ParentAgentId == parentAgentId).ToArray();

        foreach (var agent in childAgents)
        {
            agent.DividendPercentage = 0M;

            if (agents.Any(x => x.ParentAgentId == agent.Id))
                foreach (var childAgent in GetChildAgents(agent.Id, agents))
                    yield return childAgent;
        }
    }

    private async ValueTask<int> UpdateSubAgentsPathAsync(Agent agent, int nLevel, Agent newParentAgent)
    {
        const string spName = "dbo.agent_update_path";

        return await _repository.ExecuteSqlCommandAsync(spName,
            parameters: new Dictionary<string, object>
            {
                { "_agent_id", agent.Id },
                { "_agent_nlevel", nLevel },
                { "_agent_path", agent.Path.ToString() },
                { "_new_agent_path", newParentAgent.Path.ToString() }
            });
    }
}
