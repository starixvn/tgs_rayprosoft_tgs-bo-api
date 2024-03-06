using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.Core.Infrastructures.Security;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Utility;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.Agent;
using CreateAgentRequest = TGS.Services.BackOffice.Application.Models.Agent.CreateAgentRequest;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public class AgentService(
    ITgsRepository repository,
    IPasswordHandler passwordHandler)
    : BaseRepositoryOperations<ITgsRepository>(repository)
    , IAgentService
{
    private readonly IPasswordHandler _passwordHandler = passwordHandler;
    private readonly ITgsRepository _repository = repository;

    async Task IAgentService.CreateAgentAsync(CreateAgentRequest request, Agent parentAgent)
    {
        var invitationCode = InvitationCodeGenerator.NewId();

        while (await ExistsAsync<Agent>(x => x.InvitationCode == invitationCode))
            invitationCode = InvitationCodeGenerator.NewId();

        var brand = await _repository.Brands.AsQueryable().FirstAsync(x => x.Id == request.BrandId);
        var defaultWalletId = Guid.NewGuid();
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

        agent.Wallets = [
            new AgentWallet
            {
                Id = defaultWalletId,
                Agent = agent,
                Balance = 0,
                CurrencyCode = brand.Currencies.First().CurrencyCode,
                CreatedOn = DateTimeOffset.Now,
                UpdatedOn = DateTimeOffset.Now
            }
        ];

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

    public async Task<(IEnumerable<AgentsModel>, int)> GetAgentsAsync(GetAgentsRequest request)
    {
        const string spName = "dbo.agent_get_agents_with_team_counts";

        var results = await _repository.SqlQueryMultipleAsync<AgentsModel>(spName,
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
