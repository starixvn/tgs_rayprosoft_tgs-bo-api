using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class GetAgentOperationQueryService : IGetService<GetAgentOperationQuery, PagingResponse<AgentAuditLogInfo>>
{
    private readonly IAgentClient _agentClient;

    public GetAgentOperationQueryService(IAgentClient agentClient)
    {
        _agentClient = agentClient ?? throw new ArgumentNullException(nameof(agentClient));
    }

    public async ValueTask<PagingResponse<AgentAuditLogInfo>> GetAsync(GetAgentOperationQuery query, CancellationToken cancellationToken = default)
    {
        var conditions = new GetAgentAuditLogQueryConditions
        {
            AgentId = query.AgentId,
            BrandId = query.BrandId,
            From = query.From,
            To = query.To,
        };

        var paging = new Paging
        {
            IsDesc = query.PageableRequest.IsDesc,
            Page = query.PageableRequest.Page,
            Rows = query.PageableRequest.Rows,
            ShowAll = query.PageableRequest.ShowAll,
            SortBy = query.PageableRequest.SortBy,
        };

        var result = await _agentClient
            .GetAgentAuditLogsAsync(conditions, paging, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }
}
