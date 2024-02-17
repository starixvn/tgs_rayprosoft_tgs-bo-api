using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.Core.Infrastructures.Security;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Infrastructures.Utility;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Domain.Enums;
using CreateAgentRequest = TGS.Services.BackOffice.Application.Models.Agent.CreateAgentRequest;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IAgentService
{
    Task CreateAgentAsync(CreateAgentRequest request, Agent parentAgent = null);

    Task<bool> ExistsAsync(Expression<Func<Agent, bool>> predicate);

    Task<Agent> GetAgentAsync(
        Expression<Func<Agent, bool>> predicate,
        Func<IQueryable<Agent>, IQueryable<Agent>> preQuery = null);

    IQueryable<AgentAuditLog> GetAgentAuditLogQueries(
        Func<IQueryable<AgentAuditLog>, IQueryable<AgentAuditLog>> preQuery = null,
        bool tracking = false);

    IQueryable<Agent> GetAgentQueries(
        Func<IQueryable<Agent>, IQueryable<Agent>> preQuery = null,
        bool tracking = false);

    Task<(IEnumerable<AgentModel>, int)> GetAgentsAsync(GetAgentsRequest request);

    Task<IEnumerable<AgentPathModel>> GetSubAgentsAsync(Guid agentId);

    Task UpdateAgentAsync(Agent agent, UpdateAgentRequest request);
}

public class AgentService(ITgsRepository repository, IPasswordHandler passwordHandler, IAgentClient agentClient) : BaseRepositoryOperations<ITgsRepository>(repository), IAgentService
{
    private readonly IPasswordHandler _passwordHandler = passwordHandler;
    private readonly ITgsRepository _repository = repository;

    async Task IAgentService.CreateAgentAsync(CreateAgentRequest request, Agent parentAgent)
    {
        var invitationCode = InvitationCodeGenerator.NewId();

        while (await ExistsAsync<Agent>(x => x.InvitationCode == invitationCode))
            invitationCode = InvitationCodeGenerator.NewId();

        var brand = await _repository.Brands.AsQueryable().FirstAsync(x => x.Id == request.BrandId);
        var defaultWalletId = GuidExtensions.NewSequentialGuid();
        var agentId = Guid.NewGuid();

        var agent = new Agent
        {
            Id = agentId,
            Username = request.Username,
            LevelType = request.LevelType,
            Type = request.Type,
            BrandId = request.BrandId,
            Status = AgentStatus.Enabled,
            ParentAgentId = parentAgent?.Id,
            DefaultWalletId = defaultWalletId,
            AgentStatus =
                brand.AgentDividendType == AgentDividendType.MultipleLayer &&
                brand.BrandAgentDividendConditions?.Count > 0 ||
                brand.AgentDividendType == AgentDividendType.OneLayer && request.DividendPercentage > 0
                    ? AgentConfigStatus.DoneForConfigurePercentage
                    : AgentConfigStatus.WaitForConfigurePercentage,
            CommissionPercentage = request.CommissionPercentage,
            DividendPercentage = request.DividendPercentage,
            LiveDealerRebatePercentage = request.LiveDealerRebatePercentage,
            SlotRebatePercentage = request.SlotRebatePercentage,
            LotteryRebatePercentage = request.LotteryRebatePercentage,
            CardGameRebatePercentage = request.CardGameRebatePercentage,
            SportsRebatePercentage = request.SportsRebatePercentage,
            VirtualSportsRebatePercentage = request.VirtualSportsRebatePercentage,
            InvitationCode = invitationCode,
            Remarks = request.Remarks,
            CreatedByUsername = request.EditorUsername,
            UpdatedByUsername = request.EditorUsername,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now,
            RegisteredOn = DateTimeOffset.Now,
            Path = parentAgent == null ? new($"{agentId:N}") : new LTree($"{parentAgent.Path}.{agentId:N}")
        };

        var (salt, pwd) = _passwordHandler.GenerateCredential(request.Password);
        agent.Password = pwd;
        agent.Salt = salt;
        agent.WithdrawPassword = [];
        agent.WithdrawSalt = [];

        agent.Wallets = new[]
        {
            new AgentWallet
            {
                Id = defaultWalletId,
                Agent = agent,
                Balance = 0,
                CurrencyCode = brand.Currencies.First().CurrencyCode,
                CreatedOn = DateTimeOffset.Now,
                UpdatedOn = DateTimeOffset.Now
            }
        }.ToHashSet();

        agent.AgentDividendConditions = brand.BrandAgentDividendConditions.Select(x => new AgentDividendCondition
        {
            Rank = x.Rank,
            NetProfit = x.NetProfit,
            ActiveNumber = x.ActiveNumber,
            DividendPercentage = x.DividendPercentage
        }).ToHashSet();

        await CreateEntityAsync(agent);
    }

    Task<bool> IAgentService.ExistsAsync(Expression<Func<Agent, bool>> predicate) => ExistsAsync(predicate);

    Task<Agent> IAgentService.GetAgentAsync(
        Expression<Func<Agent, bool>> predicate,
        Func<IQueryable<Agent>, IQueryable<Agent>> preQuery) => GetEntityFirstOrDefaultAsync(predicate, preQuery);

    IQueryable<AgentAuditLog> IAgentService.GetAgentAuditLogQueries(
        Func<IQueryable<AgentAuditLog>, IQueryable<AgentAuditLog>> preQuery, bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    IQueryable<Agent> IAgentService.GetAgentQueries(
        Func<IQueryable<Agent>, IQueryable<Agent>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    public async Task<(IEnumerable<AgentModel>, int)> GetAgentsAsync(GetAgentsRequest request)
    {
        const string spName = "dbo.agent_get_agents_with_team_counts";

        var results = await _repository.SqlQueryMultipleAsync<AgentModel>(spName,
            parameters: new Dictionary<string, object>
            {
                { "_brand_id", request.BrandId },
                { "_username", request.Username },
                { "_page_size", request.ShowAll ? int.MaxValue : request.Rows },
                { "_page_number", request.ShowAll ? 1 : request.Page },
                { "_sort_by", request.SortBy },
                { "_is_desc", request.IsDesc },
                { "_status", request.Status },
                { "_type", request.Type },
                { "_level_type", request.LevelType }
            });

        return (results, results.Any() ? results.First().TotalCount : 0);
    }

    public Task<IEnumerable<AgentPathModel>> GetSubAgentsAsync(Guid agentId)
    {
        const string spName = "dbo.agent_get_all_sub_agents";

        return _repository.SqlQueryMultipleAsync<AgentPathModel>(spName,
            parameters: new Dictionary<string, object> { { "_agent_path", agentId.ToString("N") } });
    }

    async Task IAgentService.UpdateAgentAsync(Agent agent, UpdateAgentRequest request)
    {
        var parentAgent =
            await _repository.Agents.AsQueryable().FirstOrDefaultAsync(x =>
                x.BrandId == request.BrandId && x.Username == request.ParentUsername).ConfigureAwait(false);

        var operation = (AgentOperation)Enum.Parse(typeof(AgentOperation), request.UpdateType.ToString());

        if (request.ParentUsername.IsNotNullOrEmpty() && agent.ParentAgentId != parentAgent?.Id)
        {
            operation = AgentOperation.Agent;

            agent.Path = new LTree($"{parentAgent!.Path}.{agent.Id:N}");

            var nLevel = await _repository.Agents.Where(x => x.Id == request.Id).Select(x => x.Path.NLevel)
                .FirstOrDefaultAsync().ConfigureAwait(false);

            var agents = await _repository.Agents.AsQueryable().Where(x => x.BrandId == agent.BrandId)
                .ToArrayAsync().ConfigureAwait(false);

            var updateAgents = GetChildAgents(request.Id, agents);

            await UpdateEntitiesAsync(updateAgents).ConfigureAwait(false);

            _ = await UpdateSubAgentsPathAsync(agent, nLevel, parentAgent).ConfigureAwait(false);
        }

        if (request.UpdateType == MemberUpdateType.Info)
        {
            await DeleteEntitiesAsync(agent.AgentDividendConditions);

            agent.AgentDividendConditions = request.AgentDividendConditions.Select(x
                    => new AgentDividendCondition
                    {
                        Rank = x.Rank,
                        NetProfit = x.NetProfit,
                        ActiveNumber = x.ActiveNumber,
                        DividendPercentage = operation == AgentOperation.Agent ? 0 : x.DividendPercentage
                    })
                .ToHashSet();
        }

        await UpdateEntityAsync(agent).ConfigureAwait(false);

        switch (request.UpdateType)
        {
            case MemberUpdateType.Info:
                await agentClient.SetAgentInfoAsync(new SetAgentInfoRequest(agent.Id,
                    parentAgent?.Id,
                    (Tgs.Agent.Client.Models.Enums.AgentStatus)request.Status,
                    request.ParentUsername.IsNullOrEmpty()
                        ? (Tgs.Agent.Client.Models.Enums.AgentLevelType)AgentLevelType.Chief
                        : (Tgs.Agent.Client.Models.Enums.AgentLevelType)AgentLevelType.Agent,
                    operation == AgentOperation.Agent
                        ? (Tgs.Agent.Client.Models.Enums.AgentConfigStatus)AgentConfigStatus.WaitForConfigurePercentage
                        : (Tgs.Agent.Client.Models.Enums.AgentConfigStatus)AgentConfigStatus.DoneForConfigurePercentage,
                    request.NegativeAgentDividendCarryForward,
                    request.RealName,
                    request.Remarks,
                    request.EditorUsername));

                await agentClient.SetAgentStatusAsync(agent.Id,
                    (Tgs.Agent.Client.Models.Enums.AgentStatus)request.Status,
                    request.EditorUsername);

                await agentClient.SetAgentDividendPercentageAsync(new SetAgentDividendPercentageRequest(agent.Id,
                    operation == AgentOperation.Agent ? -1 : request.DividendPercentage));

                break;

            case MemberUpdateType.Contact:
                var contactProperties = new Dictionary<AgentPropertyName, string>
                {
                    { AgentPropertyName.Phone, request.Phone },
                    { AgentPropertyName.Email, request.Email },
                    { AgentPropertyName.Qq, request.QQ },
                    { AgentPropertyName.Wechat, request.Wechat }
                };

                await agentClient.SetAgentPropertiesAsync(agent.Id,
                    contactProperties.ToImmutableDictionary(),
                    request.EditorUsername);

                break;

            case MemberUpdateType.Password:
                await agentClient.SetAgentPasswordAsync(new SetAgentPasswordRequest(agent.Id,
                    request.Password,
                    request.EditorUsername));

                break;
        }

        await agentClient.AddAgentAuditLogAsync(new AddAgentAuditLogRequest(agent.Id,
            (Tgs.Agent.Client.Models.Enums.AgentOperation)operation,
            request.EditorUsername));
    }

    public async ValueTask<int> UpdateSubAgentsPathAsync(Agent agent, int nLevel, Agent newParentAgent)
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

    private static IEnumerable<Agent> GetChildAgents(Guid parentAgentId, Agent[] agents)
    {
        var childAgents = agents.Where(x => x.ParentAgentId == parentAgentId).ToArray();

        foreach (var agent in childAgents)
        {
            agent.DividendPercentage = 0M;

            foreach (var condition in agent.AgentDividendConditions)
                condition.DividendPercentage = 0M;

            yield return agent;

            if (agents.Any(x => x.ParentAgentId == agent.Id))
                foreach (var childAgent in GetChildAgents(agent.Id, agents))
                    yield return childAgent;
        }
    }
}
