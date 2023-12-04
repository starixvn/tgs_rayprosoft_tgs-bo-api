using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.PlayerHandlers;

internal class FundInPlayerGameProviderCommandService(IGameProvidersBackofficePluggableClient client) : ICommandService<FundInPlayerGameProviderCommand>
{
    private readonly IGameProvidersBackofficePluggableClient _client = client ?? throw new ArgumentNullException(nameof(client));

    public ValueTask ExecuteAsync(FundInPlayerGameProviderCommand command, CancellationToken cancellationToken = default)
        => _client.FundInAsync(command.PlayerCode, command.GameProviderCode, cancellationToken);
}
