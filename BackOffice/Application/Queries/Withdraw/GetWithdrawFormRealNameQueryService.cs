using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.Withdraw
{
    internal class GetWithdrawFormRealNameQueryService : IGetService<GetWithdrawFormRealNameQuery, string>
    {
        private readonly IGetService<Guid, string> _getAgentRealNameService;
        private readonly IGetService<string, string> _getPlayerRealNameService;

        public GetWithdrawFormRealNameQueryService(
            IGetService<Guid, string> getAgentRealNameService,
            IGetService<string, string> getPlayerRealNameService)
        {
            _getAgentRealNameService = getAgentRealNameService ?? throw new ArgumentNullException(nameof(getAgentRealNameService));
            _getPlayerRealNameService = getPlayerRealNameService ?? throw new ArgumentNullException(nameof(getPlayerRealNameService));
        }

        public async ValueTask<string> GetAsync(GetWithdrawFormRealNameQuery query, CancellationToken cancellationToken = default)
        {
            if (query.UserType == Tgs.Payment.Clients.UserType.Player)
            {
                var playerCode = query.UserId;

                var playerRealName = await _getPlayerRealNameService
                    .GetAsync(playerCode, cancellationToken)
                    .ConfigureAwait(false);

                return playerRealName;
            }
            else if (query.UserType == Tgs.Payment.Clients.UserType.Agent)
            {
                var agentId = Guid.Parse(query.UserId);

                var agentRealName = await _getAgentRealNameService
                    .GetAsync(agentId, cancellationToken)
                    .ConfigureAwait(false);

                return agentRealName;
            }
            return null;
        }
    }
}
