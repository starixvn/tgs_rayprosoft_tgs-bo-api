#nullable enable

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.CashService.Client;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class AgentReduceAmountCommandService(
    IAgentClient agentClient,
    ICashServiceClient cashServiceClient,
    IPlayerServiceClient playerServiceClient,
    IAdjustAgentAccountService adjustAgentAccountService,
    IAdjustTransferService adjustTransferService)
    : IGetService<AgentReduceAmountCommand, AgentAdjustAmountResult>
{
    private readonly IAdjustAgentAccountService _adjustAgentAccountService = adjustAgentAccountService ?? throw new ArgumentNullException(nameof(adjustAgentAccountService));
    private readonly IAdjustTransferService _adjustTransferService = adjustTransferService ?? throw new ArgumentNullException(nameof(adjustTransferService));
    private readonly IAgentClient _agentClient = agentClient ?? throw new ArgumentNullException(nameof(agentClient));
    private readonly ICashServiceClient _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
    private readonly IPlayerServiceClient _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));

    public async ValueTask<AgentAdjustAmountResult> GetAsync(
        AgentReduceAmountCommand command,
        CancellationToken cancellationToken = default)
    {
        var agent = await _agentClient.GetAgentInfoAsync(
            new GetAgentInfoRequest(command.AgentId),
            cancellationToken).ConfigureAwait(false);

        return agent switch
        {
            null => new AgentAdjustAmountResult(false, Application.Resources.Agent.NotFound),
            _ => IsTransferToAgent(command)
                ? await TransferToAgentAsync(
                    agent,
                    command.TargetName,
                    command.Amount,
                    command.Remark,
                    command.TransactionType,
                    command.UserId,
                    command.Username,
                    cancellationToken).ConfigureAwait(false)
                : IsTransferToPlayer(command)
                    ? await TransferToPlayerAsync(
                        agent,
                        command.TargetName,
                        command.Amount,
                        command.Remark,
                        command.TransactionType,
                        command.UserId,
                        command.Username,
                        cancellationToken).ConfigureAwait(false)
                    : await AdjustAmountAsync(
                        agent,
                        command.Amount,
                        command.Remark,
                        command.TransactionType,
                        command.UserId,
                        command.Username,
                        cancellationToken).ConfigureAwait(false)
        };
    }

    private static bool IsTransferToAgent(AgentReduceAmountCommand command)
        => command.TransactionType == AgentAdjustmentTransactionType.Transfer
            && command.AccountType == AccountType.Agent;

    private static bool IsTransferToPlayer(AgentReduceAmountCommand command)
        => command.TransactionType == AgentAdjustmentTransactionType.Transfer
            && command.AccountType == AccountType.Player;

    private async ValueTask<AgentAdjustAmountResult> TransferToAgentAsync(
        AgentInfoModel agent,
        string targetName,
        decimal amount,
        string? remark,
        AgentAdjustmentTransactionType transactionType,
        Guid modifier,
        string modifierName,
        CancellationToken cancellationToken)
    {
        var targetAgent = await _agentClient.GetAgentInfoByUsernameAsync(
            new GetAgentInfoByUsernameRequest(agent.BrandId, targetName),
            cancellationToken).ConfigureAwait(false);

        if (targetAgent is null)
            return new AgentAdjustAmountResult(false, Application.Resources.Agent.TargetAgentNotFound);

        if (!(targetAgent.ParentAgentId == agent.Id || targetAgent.Id == agent.ParentAgentId))
            return new AgentAdjustAmountResult(false, Application.Resources.Agent.TargetAgentNotHomologue);

        var lockTransferResult = await _cashServiceClient.LockTransferAsync(
            agent.WalletId!.Value,
            amount,
            remark,
            Guid.Empty,
            modifier,
            cancellationToken).ConfigureAwait(false);

        var adjustFormResult = await _adjustAgentAccountService.CreateAsync(
            lockTransferResult.TransactionId,
            agent.BrandId,
            agent.Id,
            AdjustmentType.Subtract,
            transactionType,
            AdjustTransferType.None,
            amount,
            JsonSerializer.Serialize(new
            {
                TransactionType = transactionType,
                TargetAgent = targetAgent.Username,
                TargetPlayer = default(string),
                Remark = remark
            }),
            modifierName).ConfigureAwait(false);

        var lockReceiveResult = await _cashServiceClient.LockReceiveAsync(
            targetAgent.WalletId!.Value,
            amount,
            remark,
            adjustFormResult.Id,
            modifier,
            cancellationToken).ConfigureAwait(false);

        await _adjustTransferService.CreateAsync(
            lockTransferResult.TransactionId,
            lockReceiveResult.TransactionId,
            AdjustTransferType.AgentToAgent,
            agent.Username,
            targetAgent.Username,
            amount).ConfigureAwait(false);

        _ = await _cashServiceClient.CommitTransferAsync(
            agent.WalletId!.Value,
            amount,
            remark,
            adjustFormResult.Id,
            modifier,
            cancellationToken).ConfigureAwait(false);

        _ = await _cashServiceClient.CommitReceiveAsync(
            targetAgent.WalletId!.Value,
            amount,
            remark,
            adjustFormResult.Id,
            modifier,
            cancellationToken).ConfigureAwait(false);

        return new AgentAdjustAmountResult(true, string.Empty);
    }

    private async ValueTask<AgentAdjustAmountResult> TransferToPlayerAsync(
        AgentInfoModel agent,
        string targetPlayerName,
        decimal amount,
        string? remark,
        AgentAdjustmentTransactionType transactionType,
        Guid modifier,
        string modifierName,
        CancellationToken cancellationToken)
    {
        var targetPlayer = await _playerServiceClient.GetPlayerInfoByUserNameAsync(
            agent.BrandId,
            targetPlayerName,
            cancellationToken).ConfigureAwait(false);

        if (targetPlayer is null)
            return new AgentAdjustAmountResult(false, Application.Resources.Agent.TargetPlayerNotFound);

        var playerProps = await _playerServiceClient.GetPlayerPropertiesAsync(
            targetPlayer.PlayerCode,
            cancellationToken).ConfigureAwait(false);

        if (!playerProps.TryGetValue("AgentId", out var playerAgentId)
            || !Guid.TryParse(playerAgentId, out var id)
            || id != agent.Id)
            return new AgentAdjustAmountResult(false, Application.Resources.Agent.TargetPlayerNotUnderAgent);

        var playerWalletId = Guid.Parse(playerProps["DefaultWalletId"]);

        var lockTransferResult = await _cashServiceClient.LockTransferAsync(
            agent.WalletId!.Value,
            amount,
            remark,
            Guid.Empty,
            modifier,
            cancellationToken).ConfigureAwait(false);

        var adjustFormResult = await _adjustAgentAccountService.CreateAsync(
            lockTransferResult.TransactionId,
            agent.BrandId,
            agent.Id,
            AdjustmentType.Subtract,
            transactionType,
            AdjustTransferType.None,
            amount,
            JsonSerializer.Serialize(new
            {
                TransactionType = transactionType,
                TargetPlayer = targetPlayer.UserName,
                Remark = remark
            }),
            modifierName).ConfigureAwait(false);

        var lockReceiveResult = await _cashServiceClient.LockReceiveAsync(
            playerWalletId,
            amount,
            remark,
            adjustFormResult.Id,
            modifier,
            cancellationToken).ConfigureAwait(false);

        await _adjustTransferService.CreateAsync(
            lockTransferResult.TransactionId,
            lockReceiveResult.TransactionId,
            AdjustTransferType.AgentToPlayer,
            agent.Username,
            targetPlayer.UserName,
            amount).ConfigureAwait(false);

        _ = await _cashServiceClient.CommitTransferAsync(
            agent.WalletId!.Value,
            amount,
            remark,
            adjustFormResult.Id,
            modifier,
            cancellationToken).ConfigureAwait(false);

        _ = await _cashServiceClient.CommitReceiveAsync(
            playerWalletId,
            amount,
            remark,
            adjustFormResult.Id,
            modifier,
            cancellationToken).ConfigureAwait(false);

        return new AgentAdjustAmountResult(true, string.Empty);
    }

    private async ValueTask<AgentAdjustAmountResult> AdjustAmountAsync(
        AgentInfoModel agent,
        decimal amount,
        string? remark,
        AgentAdjustmentTransactionType transactionType,
        Guid modifier,
        string modifierName,
        CancellationToken cancellationToken)
    {
        var adjustResult = await _cashServiceClient.AdjustCompanyV2Async(
            agent.WalletId!.Value,
            -amount,
            remark,
            null,
            modifier,
            cancellationToken).ConfigureAwait(false);

        _ = await _adjustAgentAccountService.CreateAsync(
            adjustResult.TransactionId,
            agent.BrandId,
            agent.Id,
            AdjustmentType.Subtract,
            transactionType,
            AdjustTransferType.None,
            amount,
            remark,
            modifierName).ConfigureAwait(false);

        return new AgentAdjustAmountResult(true, string.Empty);
    }
}
