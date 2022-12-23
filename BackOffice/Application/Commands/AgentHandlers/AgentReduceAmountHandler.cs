using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Http;
using Tgs.CashService.Client;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.AgentAdjust;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;
using TGS.Services.WalletService;

namespace TGS.Services.BackOffice.Application.Commands.AgentHandlers;

public class AgentReduceAmountHandler : Handler<CreateAgentReduceAmountRequest, MessageResponse>
{
    private readonly IAdjustAgentAccountService _adjustAgentAccountService;
    private readonly IAdjustTransferService _adjustTransferService;
    private readonly IAgentService _agentService;
    private readonly IAgentWalletRepository _agentWalletRepository;
    private readonly ICashServiceClient _cashServiceClient;
    private readonly IJsonSerializationProvider _jsonSerializationProvider;
    private readonly IPlayerService _playerService;

    public AgentReduceAmountHandler(
        IAgentService agentService,
        IAgentWalletRepository agentWalletRepository,
        ICashServiceClient cashServiceClient,
        IPlayerService playerService,
        IJsonSerializationProvider jsonSerializationProvider,
        IAdjustAgentAccountService adjustAgentAccountService,
        IAdjustTransferService adjustTransferService,
        IHttpContextAccessor accessor) : base(accessor)
    {
        _agentService = agentService;
        _agentWalletRepository = agentWalletRepository;
        _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
        _playerService = playerService;
        _jsonSerializationProvider = jsonSerializationProvider;
        _adjustAgentAccountService = adjustAgentAccountService;
        _adjustTransferService = adjustTransferService;
    }

    public override async Task<MessageResponse> Handle(
        CreateAgentReduceAmountRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.TransactionType) ||
            !Enum.IsDefined((AgentAdjustmentTransactionType)request.TransactionType))
        {
            return CreateInvalidResponse<MessageResponse>("is not define type");
        }

        var agent = await _agentService.GetAgentAsync(x => x.Id == request.AgentId).ConfigureAwait(false);

        if (agent is null)
        {
            return CreateInvalidResponse<MessageResponse>(Agent.NotFound);
        }

        var targetAgent = IsTransferToAgent(request)
            ? await _agentService.GetAgentAsync(x => x.Username == request.TargetName).ConfigureAwait(false)
            : null;

        if (IsTransferToAgent(request))
        {
            if (targetAgent is null)
            {
                return CreateInvalidResponse<MessageResponse>(Agent.TargetAgentNotFound);
            }

            if (!(targetAgent.ParentAgentId == agent.Id || targetAgent.Id == agent.ParentAgentId))
            {
                return CreateInvalidResponse<MessageResponse>(Agent.TargetAgentNotHomologue);
            }
        }

        var targetPlayer = IsTransferToPlayer(request)
            ? await _playerService.GetPlayerAsync(x => x.Username == request.TargetName).ConfigureAwait(false)
            : null;

        if (IsTransferToPlayer(request))
        {
            if (targetPlayer is null)
            {
                return CreateInvalidResponse<MessageResponse>(Agent.TargetPlayerNotFound);
            }

            if (targetPlayer.AgentId != agent.Id)
            {
                return CreateInvalidResponse<MessageResponse>(Agent.TargetPlayerNotUnderAgent);
            }
        }

        var remark = _jsonSerializationProvider.SerializeToString(new
        {
            request.TransactionType,
            TargetAgent = targetAgent?.Username,
            TargetPlayer = targetPlayer?.Username,
            request.Remark
        });

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var (transactionId, postBalance) = await _agentWalletRepository.ExchangeAsync(
            agent.BrandId,
            agent.DefaultWalletId,
            agent.Id,
            TransactionTypes.CompanyAdjust,
            -request.Amount,
            remark).ConfigureAwait(false);

        _ = await _adjustAgentAccountService.CreateAsync(
            transactionId,
            agent.BrandId,
            agent.Id,
            AdjustmentType.Add,
            (AgentAdjustmentTransactionType)request.TransactionType,
            AdjustTransferType.None,
            request.Amount,
            request.Remark,
            Username).ConfigureAwait(false);

        if (IsTransferToAgent(request))
        {
            var (transId, _) = await _agentWalletRepository.ExchangeAsync(
                targetAgent.BrandId,
                targetAgent.DefaultWalletId,
                targetAgent.Id,
                TransactionTypes.CompanyAdjust,
                request.Amount).ConfigureAwait(false);

            await _adjustTransferService.CreateAsync(transactionId,
                transId,
                AdjustTransferType.AgentToAgent,
                agent.Username,
                targetAgent.Username,
                request.Amount).ConfigureAwait(false);
        }

        if (IsTransferToPlayer(request))
        {
            var playerTargetResponse = await _cashServiceClient
                .AdjustTransferAsync(
                    targetPlayer.DefaultWalletId,
                    request.Amount,
                    request.Remark,
                    null,
                    Guid.Parse(UserId),
                    cancellationToken)
                .ConfigureAwait(false);

            await _adjustTransferService.CreateAsync(transactionId,
                playerTargetResponse.TransactionId,
                AdjustTransferType.AgentToPlayer,
                agent.Username,
                targetPlayer.Username,
                request.Amount).ConfigureAwait(false);
        }

        scope.Complete();

        return CreateValidResponse<MessageResponse>();
    }

    private static bool IsTransferToAgent(CreateAgentReduceAmountRequest request) =>
        request.TransactionType == AgentReduceTransactionType.Transfer &&
        request.AccountType == AccountType.Agent;

    private static bool IsTransferToPlayer(CreateAgentReduceAmountRequest request) =>
        request.TransactionType == AgentReduceTransactionType.Transfer &&
        request.AccountType == AccountType.Player;
}
