using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.Agent;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IAgentBankService
{
    Task CreateAgentBankAsync(CreateAgentBankRequest request);

    Task DeleteAgentBankAsync(AgentBank agentBank);

    Task<bool> ExistsAsync(Expression<Func<AgentBank, bool>> predicate);

    Task<AgentBank> GetAgentBankAsync(Expression<Func<AgentBank, bool>> predicate,
                                Func<IQueryable<AgentBank>, IQueryable<AgentBank>> preQuery = null);

    IQueryable<AgentBank> GetAgentBanks(Func<IQueryable<AgentBank>, IQueryable<AgentBank>> preQuery = null,
        bool tracking = false);

    Task UpdateAgentBankAsync(AgentBank agentBank, UpdateAgentBankRequest request);
}

public class AgentBankService(ITgsRepository repository) : BaseRepositoryOperations<ITgsRepository>(repository), IAgentBankService
{
    async Task IAgentBankService.CreateAgentBankAsync(CreateAgentBankRequest request)
    {
        var agentBank = new AgentBank
        {
            AgentId = request.AgentId,
            BankName = request.BankName,
            RealName = request.RealName,
            BankAccount = request.BankAccount
        };

        await CreateEntityAsync(agentBank);
    }

    Task IAgentBankService.DeleteAgentBankAsync(AgentBank agentBank) => DeleteEntityAsync(agentBank);

    Task<bool> IAgentBankService.ExistsAsync(Expression<Func<AgentBank, bool>> predicate) => ExistsAsync(predicate);

    Task<AgentBank> IAgentBankService.GetAgentBankAsync(Expression<Func<AgentBank, bool>> predicate,
        Func<IQueryable<AgentBank>, IQueryable<AgentBank>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    IQueryable<AgentBank> IAgentBankService.GetAgentBanks(
        Func<IQueryable<AgentBank>, IQueryable<AgentBank>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    async Task IAgentBankService.UpdateAgentBankAsync(AgentBank agentBank, UpdateAgentBankRequest request)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        agentBank.RealName = request.RealName;
        agentBank.BankName = request.BankName;
        agentBank.BankAccount = request.BankAccount;
        agentBank.UpdatedByUsername = request.EditorUsername;
        agentBank.UpdatedOn = DateTimeOffset.Now;

        var auditLog = new AgentAuditLog
        {
            Id = GuidExtensions.NewSequentialGuid(),
            AgentId = agentBank.AgentId,
            CreatedByUsername = request.EditorUsername,
            CreatedOn = DateTimeOffset.Now,
            Operation = AgentOperation.Bank
        };

        await UpdateEntityAsync(agentBank).ConfigureAwait(false);
        await CreateEntityAsync(auditLog).ConfigureAwait(false);

        scope.Complete();
    }
}
