# nullable enable

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

internal class SetPlayerContactCommandService(
    IPlayerServiceClient playerServiceClient,
    ITgsRepository tgsRepository) : IGetService<SetPlayerContactCommand, SetPlayerContactInfo>
{
    public async ValueTask<SetPlayerContactInfo> GetAsync(SetPlayerContactCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.ModifyContact.Any())
            return new SetPlayerContactInfo(false, string.Empty);

        var player = await playerServiceClient
            .GetPlayerInfoAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        if (player is null)
            return new SetPlayerContactInfo(false, Application.Resources.Player.NotFound);

        var props = await playerServiceClient
            .GetPlayerPropertiesAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        _ = props.TryGetValue("Phone", out var playerPhone);

        var modifyDict = new Dictionary<PlayerPropertyName, string?>();

        var availablePropertyNames = new HashSet<string>(
            new string[]
            {
                nameof(PlayerPropertyName.AreaCode),
                nameof(PlayerPropertyName.Phone),
                nameof(PlayerPropertyName.Email),
                nameof(PlayerPropertyName.Qq),
                nameof(PlayerPropertyName.Wechat),
                nameof(PlayerPropertyName.Telegram),
                nameof(PlayerPropertyName.Viber),
                nameof(PlayerPropertyName.FacebookMessenger),
                nameof(PlayerPropertyName.Line),
                nameof(PlayerPropertyName.WhatsApp),
                nameof(PlayerPropertyName.PhoneAuthentication),
                nameof(PlayerPropertyName.Zalo)
            },
            StringComparer.OrdinalIgnoreCase);

        foreach (var item in command.ModifyContact)
        {
            if (availablePropertyNames.Contains(item.Key))
            {
                if (!string.IsNullOrEmpty(playerPhone) && item.Key.Equals("phone", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(item.Value))
                {
                    return new SetPlayerContactInfo(false, Application.Resources.Player.PhoneRequired);
                }

                if (item.Key.Equals("phone", StringComparison.OrdinalIgnoreCase))
                {
                    modifyDict[PlayerPropertyName.PhoneAuthentication] = (!string.IsNullOrEmpty(item.Value)).ToString();
                }

                if (item.Key.Equals("email", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(item.Value))
                {
                    var playerInfo = await playerServiceClient
                        .GetPlayerInfoByEmailAsync(player.BrandId, item.Value, cancellationToken)
                        .ConfigureAwait(false);

                    if (playerInfo is not null && player.PlayerCode != playerInfo.PlayerCode)
                    {
                        return new SetPlayerContactInfo(false, Application.Resources.Player.BrandEmailDuplicate);
                    }
                }

                modifyDict.Add(Enum.Parse<PlayerPropertyName>(item.Key, true), item.Value);

                if (item.Key.Equals("email", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(item.Value))
                {
                    modifyDict[PlayerPropertyName.Email] = null;
                }
            }
        }

        if (modifyDict.Count == 0)
            return new SetPlayerContactInfo(false, string.Empty);

        await playerServiceClient
            .SetPlayerPropertiesAsync(command.PlayerCode, modifyDict.ToImmutableDictionary(), command.ModifyUserId, cancellationToken)
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

        _ = await tgsRepository.PlayerAuditLogs
            .AddAsync(auditLog, cancellationToken)
            .ConfigureAwait(false);

        _ = await tgsRepository
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return new SetPlayerContactInfo(true, string.Empty);
    }
}
