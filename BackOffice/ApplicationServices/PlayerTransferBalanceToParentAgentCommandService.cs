using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.ApplicationServices.Agents;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class PlayerTransferBalanceToParentAgentCommandService : ICommandService<PlayerTransferBalanceToParentAgentCommand>
{
    private readonly IPlayerAdjustBalanceService _playerAdjustBalanceService;
    private readonly IPlayerServiceClient _playerServiceClient;
    private readonly IQueryService<AllAgentsByBrandQuery, AgentInfo> _queryAgentsService;

    public PlayerTransferBalanceToParentAgentCommandService(
        IPlayerServiceClient playerServiceClient,
        IQueryService<AllAgentsByBrandQuery, AgentInfo> queryAgentsService,
        IPlayerAdjustBalanceService playerAdjustBalanceService)
    {
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        _queryAgentsService = queryAgentsService ?? throw new ArgumentNullException(nameof(queryAgentsService));
        _playerAdjustBalanceService = playerAdjustBalanceService ?? throw new ArgumentNullException(nameof(playerAdjustBalanceService));
    }

    public async ValueTask ExecuteAsync(PlayerTransferBalanceToParentAgentCommand command, CancellationToken cancellationToken = default)
    {
        var playerInfo = await _playerServiceClient
            .GetPlayerInfoAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        if (playerInfo is null)
            throw new Exception(Player.NotFound);

        var props = await _playerServiceClient
            .GetPlayerPropertiesAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        if (!props.TryGetValue("DefaultWalletId", out var walletId))
            throw new Exception(Agent.WalletNotFound);

        if (!props.TryGetValue("AgentId", out var agentId))
            throw new Exception(Agent.TargetAgentNotFound);

        var targetAgent = await _queryAgentsService.QueryAsync(new AllAgentsByBrandQuery(playerInfo.BrandId), cancellationToken)
            .FirstOrDefaultAsync(
                agent => agent.AgentId == Guid.Parse(agentId),
                cancellationToken)
            .ConfigureAwait(false);

        if (targetAgent is null)
            throw new Exception(Agent.TargetAgentNotFound);

        var apply = new PlayerBalanceTransferApplyInfo(
            playerInfo.BrandId,
            playerInfo.PlayerCode,
            Guid.Parse(walletId),
            command.Amount,
            command.BucketAmount,
            command.IncludeInNetProfit,
            command.Remark,
            targetAgent.AgentId,
            targetAgent.WalletId);

        await _playerAdjustBalanceService
            .ProcessTransferApplyAsync(apply, command.ModifyUserId, cancellationToken)
            .ConfigureAwait(false);
    }
}
