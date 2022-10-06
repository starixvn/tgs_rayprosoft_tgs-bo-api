using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.Withdraw
{
    internal class GetWithdrawFormRealNameQueryService : IGetService<GetWithdrawFormRealNameQuery, string>
    {
        private readonly IGetService<Guid, string> _getAgentService;
        private readonly IGetService<string, IImmutableDictionary<string, string>> _getPlayerPropertiesService;

        public GetWithdrawFormRealNameQueryService(
            IGetService<Guid, string> getAgentService,
            IGetService<string, IImmutableDictionary<string, string>> getPlayerPropertiesService)
        {
            _getAgentService = getAgentService ?? throw new ArgumentNullException(nameof(getAgentService));
            _getPlayerPropertiesService = getPlayerPropertiesService ?? throw new ArgumentNullException(nameof(getPlayerPropertiesService));
        }

        public async ValueTask<string> GetAsync(GetWithdrawFormRealNameQuery query, CancellationToken cancellationToken = default)
        {
            var realName = string.Empty;

            if (query.UserType == Tgs.Payment.Clients.UserType.Player)
            {
                var playerCode = query.UserId;

                var playerProperties = await _getPlayerPropertiesService
                    .GetAsync(playerCode, cancellationToken)
                    .ConfigureAwait(false);

                realName = playerProperties["RealName"];
            }
            else if (query.UserType == Tgs.Payment.Clients.UserType.Agent)
            {
                var agentId = Guid.Parse(query.UserId);

                var agentUsername = await _getAgentService
                    .GetAsync(agentId, cancellationToken)
                    .ConfigureAwait(false);

                realName = agentUsername;
            }

            return realName;
        }
    }
}
