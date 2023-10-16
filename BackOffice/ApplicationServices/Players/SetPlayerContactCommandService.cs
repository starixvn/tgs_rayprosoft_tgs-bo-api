# nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class SetPlayerContactCommandService : ICommandService<SetPlayerContactCommand>
{
    private readonly IPlayerServiceClient _playerServiceClient;
    private readonly ITgsRepository _tgsRepository;

    public SetPlayerContactCommandService(
        IPlayerServiceClient playerServiceClient,
        ITgsRepository tgsRepository)
    {
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
    }

    public async ValueTask ExecuteAsync(SetPlayerContactCommand command, CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<PlayerPropertyName, string?>();

        var availablePropertyNames = new HashSet<string>(
            new string[]
            {
                PlayerPropertyName.AreaCode.ToString(),
                PlayerPropertyName.Phone.ToString(),
                PlayerPropertyName.Email.ToString(),
                PlayerPropertyName.Qq.ToString(),
                PlayerPropertyName.Wechat.ToString(),
                PlayerPropertyName.Telegram.ToString(),
                PlayerPropertyName.Viber.ToString(),
                PlayerPropertyName.FacebookMessenger.ToString(),
                PlayerPropertyName.Line.ToString(),
                PlayerPropertyName.WhatsApp.ToString(),
                PlayerPropertyName.PhoneAuthentication.ToString(),
            },
            StringComparer.OrdinalIgnoreCase);

        foreach (var item in command.ModifyContact)
        {
            if (availablePropertyNames.Contains(item.Key))
            {
                dict.Add(Enum.Parse<PlayerPropertyName>(item.Key), item.Value);

                if (item.Key.ToLower() == "phone" && !string.IsNullOrEmpty(item.Value))
                    dict.Add(PlayerPropertyName.PhoneAuthentication, true.ToString());
            }
        }

        await _playerServiceClient
            .SetPlayerPropertiesAsync(command.PlayerCode, dict.ToImmutableDictionary(), command.ModifyUserId, cancellationToken)
            .ConfigureAwait(false);

        var auditLog = new PlayerAuditLog
        {
            Id = GuidExtensions.NewSequentialGuid(),
            ModuleName = "Player",
            CreatedByUsername = command.ModifyUsername,
            CreatedOn = DateTimeOffset.Now,
            Operation = "Update Player contact",
            Username = command.PlayerCode
        };

        _ = await _tgsRepository.PlayerAuditLogs
            .AddAsync(auditLog, cancellationToken)
            .ConfigureAwait(false);

        _ = await _tgsRepository
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
