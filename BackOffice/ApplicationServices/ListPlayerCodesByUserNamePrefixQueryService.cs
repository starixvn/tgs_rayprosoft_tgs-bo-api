using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class ListPlayerCodesByUserNamePrefixQueryService(IPlayerServiceClient playerServiceClient)
        : IQueryService<ListPlayerCodesByUserNamePrefixQuery, KeyValuePair<string, string>>
{
    private readonly IPlayerServiceClient _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));

    public IAsyncEnumerable<KeyValuePair<string, string>> QueryAsync(
        ListPlayerCodesByUserNamePrefixQuery query,
        CancellationToken cancellationToken = default)
        => _playerServiceClient.ListPlayerCodesByUserNameAsync(
            query.BrandId,
            query.UserNamePrefix,
            cancellationToken);
}
