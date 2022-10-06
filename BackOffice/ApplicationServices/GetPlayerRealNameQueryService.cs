using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices
{
    internal class GetPlayerRealNameQueryService : IGetService<GetPlayerRealNameQuery, string>
    {
        private readonly IPlayerServiceClient _playerServiceClient;

        public GetPlayerRealNameQueryService(IPlayerServiceClient playerServiceClient)
        {
            _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        }

        public async ValueTask<string> GetAsync(GetPlayerRealNameQuery query, CancellationToken cancellationToken = default)
        {
            var playerProperties = await _playerServiceClient
                .GetPlayerPropertiesAsync(query.PlayerCode, cancellationToken)
                .ConfigureAwait(false);

            return playerProperties["RealName"];
        }
    }
}
