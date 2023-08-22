using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tgs.CashService.Client;
using Tgs.ServiceUtility;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;
using TGS.Services.WalletService;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class AgentReduceAmountQueryService : IGetService<AgentReduceAmountQuery, AgentReduceAmountResult>
{
    private readonly IAdjustAgentAccountService _adjustAgentAccountService;
    private readonly IAdjustTransferService _adjustTransferService;
    private readonly IAgentService _agentService;
    private readonly IAgentWalletRepository _agentWalletRepository;
    private readonly ICashServiceClient _cashServiceClient;
    private readonly IJsonSerializationProvider _jsonSerializationProvider;
    private readonly IPlayerService _playerService;

    public AgentReduceAmountQueryService(
        IAgentService agentService,
        IAgentWalletRepository agentWalletRepository,
        ICashServiceClient cashServiceClient,
        IPlayerService playerService,
        IJsonSerializationProvider jsonSerializationProvider,
        IAdjustAgentAccountService adjustAgentAccountService,
        IAdjustTransferService adjustTransferService)
    {
        _agentService = agentService ?? throw new ArgumentNullException(nameof(agentService));
        _agentWalletRepository = agentWalletRepository ?? throw new ArgumentNullException(nameof(agentWalletRepository));
        _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
        _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
        _jsonSerializationProvider = jsonSerializationProvider ?? throw new ArgumentNullException(nameof(jsonSerializationProvider));
        _adjustAgentAccountService = adjustAgentAccountService ?? throw new ArgumentNullException(nameof(adjustAgentAccountService));
        _adjustTransferService = adjustTransferService ?? throw new ArgumentNullException(nameof(adjustTransferService));
    }

    public async ValueTask<AgentReduceAmountResult> GetAsync(AgentReduceAmountQuery query, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(query.TransactionType) ||
            !Enum.IsDefined((AgentAdjustmentTransactionType)query.TransactionType))
        {
            return new AgentReduceAmountResult(false, "is not define type");
        }

        var agent = await _agentService.GetAgentAsync(x => x.Id == query.AgentId).ConfigureAwait(false);

        if (agent is null)
        {
            return new AgentReduceAmountResult(false, Agent.NotFound);
        }

        var targetAgent = IsTransferToAgent(query)
            ? await _agentService.GetAgentAsync(x => x.Username == query.TargetName && x.BrandId == agent.BrandId).ConfigureAwait(false)
            : null;

        if (IsTransferToAgent(query))
        {
            if (targetAgent is null)
                return new AgentReduceAmountResult(false, Agent.TargetAgentNotFound);

            if (!(targetAgent.ParentAgentId == agent.Id || targetAgent.Id == agent.ParentAgentId))
                return new AgentReduceAmountResult(false, Agent.TargetAgentNotHomologue);
        }

        var targetPlayer = IsTransferToPlayer(query)
            ? await _playerService.GetPlayerAsync(x => x.Username == query.TargetName && x.BrandId == agent.BrandId).ConfigureAwait(false)
            : null;

        if (IsTransferToPlayer(query))
        {
            if (targetPlayer is null)
                return new AgentReduceAmountResult(false, Agent.TargetPlayerNotFound);

            if (targetPlayer.AgentId != agent.Id)
                return new AgentReduceAmountResult(false, Agent.TargetPlayerNotUnderAgent);
        }

        var remark = _jsonSerializationProvider.SerializeToString(new
        {
            query.TransactionType,
            TargetAgent = targetAgent?.Username,
            TargetPlayer = targetPlayer?.Username,
            query.Remark
        });

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var (transactionId, postBalance) = await _agentWalletRepository.ExchangeAsync(
            agent.BrandId,
            agent.DefaultWalletId,
            agent.Id,
            TransactionTypes.CompanyAdjust,
            -query.Amount,
            remark).ConfigureAwait(false);

        _ = await _adjustAgentAccountService.CreateAsync(
            transactionId,
            agent.BrandId,
            agent.Id,
            AdjustmentType.Add,
            (AgentAdjustmentTransactionType)query.TransactionType,
            AdjustTransferType.None,
            query.Amount,
            query.Remark,
            query.Username).ConfigureAwait(false);

        if (IsTransferToAgent(query))
        {
            var (transId, _) = await _agentWalletRepository.ExchangeAsync(
                targetAgent.BrandId,
                targetAgent.DefaultWalletId,
                targetAgent.Id,
                TransactionTypes.CompanyAdjust,
                query.Amount).ConfigureAwait(false);

            await _adjustTransferService.CreateAsync(transactionId,
                transId,
                AdjustTransferType.AgentToAgent,
                agent.Username,
                targetAgent.Username,
                query.Amount).ConfigureAwait(false);
        }

        if (IsTransferToPlayer(query))
        {
            var playerTargetResponse = await _cashServiceClient
                .AdjustTransferAsync(
                    targetPlayer.DefaultWalletId,
                    query.Amount,
                    query.Remark,
                    null,
                    Guid.Parse(query.UserId),
                    cancellationToken)
                .ConfigureAwait(false);

            await _adjustTransferService.CreateAsync(
                transactionId,
                playerTargetResponse.TransactionId,
                AdjustTransferType.AgentToPlayer,
                agent.Username,
                targetPlayer.Username,
                query.Amount).ConfigureAwait(false);
        }

        scope.Complete();

        return new AgentReduceAmountResult(true, string.Empty);
    }

    private static bool IsTransferToAgent(AgentReduceAmountQuery query)
        => query.TransactionType == AgentReduceTransactionType.Transfer
            && query.AccountType == AccountType.Agent;

    private static bool IsTransferToPlayer(AgentReduceAmountQuery query)
        => query.TransactionType == AgentReduceTransactionType.Transfer
            && query.AccountType == AccountType.Player;
}
