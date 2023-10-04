using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

#nullable enable

namespace TGS.Services.BackOffice.ApplicationServices;

internal class UpdatePlayerIdentityAuthenticationCommandService : ICommandService<UpdatePlayerIdentityAuthenticationCommand>
{
    private readonly IPlayerServiceClient _playerServiceClient;

    public UpdatePlayerIdentityAuthenticationCommandService(IPlayerServiceClient playerServiceClient)
    {
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
    }

    public async ValueTask ExecuteAsync(UpdatePlayerIdentityAuthenticationCommand command, CancellationToken cancellationToken = default)
    {
        var properties = new Dictionary<PlayerPropertyName, string?>
        {
            [PlayerPropertyName.IdentityAuthentication] = command.IdentityAuthentication.ToString()
        };

        await _playerServiceClient.SetPlayerPropertiesAsync(
            command.PlayerCode,
            properties.ToImmutableDictionary(),
            command.ModifyUser,
            cancellationToken).ConfigureAwait(false);
    }
}
