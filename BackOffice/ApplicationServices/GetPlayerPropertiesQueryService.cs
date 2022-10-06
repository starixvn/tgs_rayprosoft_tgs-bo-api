using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices
{
    internal class GetPlayerPropertiesQueryService : IGetService<string, IImmutableDictionary<string, string>>
    {
        private readonly IPlayerServiceClient _playerServiceClient;

        public GetPlayerPropertiesQueryService(IPlayerServiceClient playerServiceClient)
        {
            _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        }

        public ValueTask<IImmutableDictionary<string, string>> GetAsync(string playerCode, CancellationToken cancellationToken = default)
            => _playerServiceClient.GetPlayerPropertiesAsync(playerCode, cancellationToken);
    }
}
