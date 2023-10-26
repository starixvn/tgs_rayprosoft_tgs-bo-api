# nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.Agent.Client.Models.Enums;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;
using Resources = TGS.Services.BackOffice.Application.Resources;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class SetPlayerAgentCommandService : IGetService<SetPlayerAgentCommand, SetPlayerAgentResult>
{
    private readonly IAgentClient _agentClient;
    private readonly IPlayerServiceClient _playerServiceClient;
    private readonly ITgsRepository _repository;

    public SetPlayerAgentCommandService(
        IPlayerServiceClient playerServiceClient,
        IAgentClient agentClient,
        ITgsRepository repository)
    {
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        _agentClient = agentClient ?? throw new ArgumentNullException(nameof(agentClient));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async ValueTask<SetPlayerAgentResult> GetAsync(SetPlayerAgentCommand command, CancellationToken cancellationToken = default)
    {
        var player = await _playerServiceClient
            .GetPlayerInfoAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        if (player is null)
            return new SetPlayerAgentResult(false, Resources.Player.NotFound);

        string? agentId = null;
        if (!string.IsNullOrWhiteSpace(command.AgentUsername))
        {
            var agent = await _agentClient
                .GetAgentInfoByUsernameAsync(new GetAgentInfoByUsernameRequest(player.BrandId, command.AgentUsername), cancellationToken)
                .ConfigureAwait(false);

            if (agent is null || agent.Status != AgentStatus.Enabled)
                return new SetPlayerAgentResult(false, Resources.Agent.NotFound);

            agentId = agent.Id.ToString();
        }

        var modifyDict = new Dictionary<PlayerPropertyName, string?>()
        {
            { PlayerPropertyName.AgentId, agentId }
        };

        await _playerServiceClient
           .SetPlayerPropertiesAsync(command.PlayerCode, modifyDict.ToImmutableDictionary(), command.ModifyUserId, cancellationToken)
           .ConfigureAwait(false);

        var auditLog = new PlayerAuditLog
        {
            Id = GuidExtensions.NewSequentialGuid(),
            ModuleName = "Player",
            CreatedByUsername = command.ModifyUsername,
            CreatedOn = DateTimeOffset.Now,
            Operation = "Update Player info",
            Username = command.PlayerCode
        };

        _ = await _repository.PlayerAuditLogs
            .AddAsync(auditLog, cancellationToken)
            .ConfigureAwait(false);

        _ = await _repository
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return new SetPlayerAgentResult(true, string.Empty);
    }
}
