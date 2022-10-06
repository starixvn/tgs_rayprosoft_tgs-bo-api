using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.ApplicationServices
{
    internal class GetAgentUsernameQueryService : IGetService<Guid, string>
    {
        private readonly IAgentService _agentService;

        public GetAgentUsernameQueryService(IAgentService agentService)
        {
            _agentService = agentService ?? throw new ArgumentNullException(nameof(agentService));
        }

        public async ValueTask<string> GetAsync(Guid agentId, CancellationToken cancellationToken = default)
        {
            var agent = await _agentService
                .GetAgentAsync(agent => agent.Id == agentId)
                .ConfigureAwait(false);

            return agent.Username;
        }
    }
}
