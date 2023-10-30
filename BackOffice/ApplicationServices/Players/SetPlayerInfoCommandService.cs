#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class SetPlayerInfoCommandService : IGetService<SetPlayerInfoCommand, SetPlayerInfoResult>
{
    private readonly IPlayerServiceClient _playerServiceClient;
    private readonly ITgsRepository _repository;

    public SetPlayerInfoCommandService(
        IPlayerServiceClient playerServiceClient,
        ITgsRepository repository)
    {
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async ValueTask<SetPlayerInfoResult> GetAsync(SetPlayerInfoCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.ModifyInfo.Any())
            return new SetPlayerInfoResult(false, string.Empty);

        var player = await _playerServiceClient
            .GetPlayerInfoAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        if (player is null)
            return new SetPlayerInfoResult(false, Application.Resources.Player.NotFound);

        var availablePropertyNames = new HashSet<string>(
            new string[]
            {
                nameof(PlayerPropertyName.RealName),
                nameof(PlayerPropertyName.Birthdate),
                nameof(PlayerPropertyName.Remark),
                nameof(PlayerPropertyName.VipId),
            },
            StringComparer.OrdinalIgnoreCase);

        var modifyDict = new Dictionary<PlayerPropertyName, string?>();

        foreach (var item in command.ModifyInfo)
        {
            if (availablePropertyNames.Contains(item.Key))
            {
                modifyDict.Add(Enum.Parse<PlayerPropertyName>(item.Key, true), item.Value);
            }
        }

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

        return new SetPlayerInfoResult(true, string.Empty);
    }
}
