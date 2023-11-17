# nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class SetAgentMultipleLayerCommandService : IGetService<SetAgentMultipleLayerCommand, SetAgentMultipleLayerResult>
{
    private readonly IAgentClient _agentClient;
    private readonly ITgsRepository _repository;

    public SetAgentMultipleLayerCommandService(
        IAgentClient agentClient,
        ITgsRepository repository)
    {
        _agentClient = agentClient ?? throw new ArgumentNullException(nameof(agentClient));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async ValueTask<SetAgentMultipleLayerResult> GetAsync(SetAgentMultipleLayerCommand command, CancellationToken cancellationToken = default)
    {
        var operation = Tgs.Agent.Client.Models.Enums.AgentOperation.Info;

        var agent = await _repository.Agents.Where(x => x.Id == command.AgentId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (agent is null)
            return new SetAgentMultipleLayerResult(false, Application.Resources.Agent.NotFound);

        var parentAgent = await _repository.Agents.AsQueryable()
            .FirstOrDefaultAsync(x => x.BrandId == agent.BrandId && x.Username == command.ParentUsername, cancellationToken)
            .ConfigureAwait(false);

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

        if (agent.Brand.AgentDividendType == AgentDividendType.MultipleLayer)
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

        var nLevel = await _repository.Agents
                .Where(x => x.Id == command.AgentId)
                .Select(x => x.Path.NLevel)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

        if (!string.IsNullOrEmpty(command.ParentUsername) && agent.ParentAgentId != parentAgent?.Id)
        {
            operation = Tgs.Agent.Client.Models.Enums.AgentOperation.Agent;

            var agents = await _repository.Agents.AsQueryable()
                .Where(x => x.BrandId == agent.BrandId)
                .ToArrayAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var updateAgents = GetChildAgents(command.AgentId, agents);
            _repository.Set<Agent>().UpdateRange(updateAgents);

            _ = await UpdateSubAgentsPathAsync(agent, nLevel, parentAgent!)
                .ConfigureAwait(false);
        }

        _repository.Set<AgentDividendCondition>().RemoveRange(agent.AgentDividendConditions);

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

        agent.AgentStatus = operation == Tgs.Agent.Client.Models.Enums.AgentOperation.Agent
            ? AgentConfigStatus.WaitForConfigurePercentage
            : AgentConfigStatus.DoneForConfigurePercentage;

        agent.AgentDividendConditions = command.AgentDividendConditions.Select(x
            => new AgentDividendCondition
            {
                Rank = x.Rank,
                NetProfit = x.NetProfit,
                ActiveNumber = x.ActiveNumber,
                DividendPercentage = operation == Tgs.Agent.Client.Models.Enums.AgentOperation.Agent ? 0 : x.DividendPercentage
            }).ToHashSet();

        agent.UpdatedByUsername = command.ModifyUsername;
        agent.UpdatedOn = DateTimeOffset.Now;

        _repository.Entry(agent).State = EntityState.Modified;

        _ = await _repository
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        await _agentClient
            .AddAgentAuditLogAsync(new AddAgentAuditLogRequest(command.AgentId, operation, command.ModifyUsername), cancellationToken)
            .ConfigureAwait(false);

        return new SetAgentMultipleLayerResult(true, string.Empty);
    }

    private static IEnumerable<Agent> GetChildAgents(Guid parentAgentId, Agent[] agents)
    {
        var childAgents = agents.Where(x => x.ParentAgentId == parentAgentId).ToArray();

        foreach (var agent in childAgents)
        {
            foreach (var condition in agent.AgentDividendConditions)
                condition.DividendPercentage = 0M;

            yield return agent;

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
