using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices
{
    internal class GetPlayerRealNameQueryService : IGetService<string, string>
    {
        private readonly IPlayerServiceClient _playerServiceClient;

        public GetPlayerRealNameQueryService(IPlayerServiceClient playerServiceClient)
        {
            _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        }

        public async ValueTask<string> GetAsync(string playerCode, CancellationToken cancellationToken = default)
        {
            var playerProperties = await _playerServiceClient
                .GetPlayerPropertiesAsync(playerCode, cancellationToken)
                .ConfigureAwait(false);

            return playerProperties["RealName"];
        }
    }
}
