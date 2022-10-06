using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.Withdraw
{
    internal class GetWithdrawFormRealNameQueryService : IGetService<GetWithdrawFormRealNameQuery, string>
    {
        private readonly IAgentService _agentService;
        private readonly IPlayerServiceClient _playerServiceClient;

        public GetWithdrawFormRealNameQueryService(
            IPlayerServiceClient playerServiceClient,
            IAgentService agentService)
        {
            _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
            _agentService = agentService ?? throw new ArgumentNullException(nameof(agentService));
        }

        public async ValueTask<string> GetAsync(GetWithdrawFormRealNameQuery query, CancellationToken cancellationToken = default)
        {
            var realName = string.Empty;

            if (query.UserType == Tgs.Payment.Clients.UserType.Player)
            {
                var playerProperties = await _playerServiceClient
                   .GetPlayerPropertiesAsync(query.UserId, cancellationToken)
                   .ConfigureAwait(false);

                realName = playerProperties["RealName"];
            }
            else if (query.UserType == Tgs.Payment.Clients.UserType.Agent)
            {
                var agentId = Guid.Parse(query.UserId);
                var agent = await _agentService
                    .GetAgentAsync(a => a.Id == agentId)
                    .ConfigureAwait(false);

                realName = agent.Username;
            }

            return realName;
        }
    }
}
