using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.AgentHandlers;

public class GetAgentOperationsQueryHandler : Handler<GetAgentOperationsRequest, PagedResponse<AgentOperationModel>>
{
    private readonly IAgentService _service;

    public GetAgentOperationsQueryHandler(IAgentService service, IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
    }

    public override Task<PagedResponse<AgentOperationModel>> Handle(GetAgentOperationsRequest request,
        CancellationToken cancellationToken) =>
        _service.GetAgentAuditLogQueries(q => q
                .Where(ResellerId, r => r.Agent.Brand.Licensee.ResellerId == ResellerId)
                .Where(LicenseeId, l => l.Agent.Brand.LicenseeId == LicenseeId)
                .Where(BrandId, l => l.Agent.Brand.Id == BrandId)
                .Where(x => x.AgentId == request.AgentId)
                .Where(request.From, x => x.CreatedOn >= request.From)
                .Where(request.To, x => x.CreatedOn <= request.To)
            )
            .Select(x => new AgentOperationModel
            {
                Id = x.Id,
                Operation = x.Operation,
                CreatedByUsername = x.CreatedByUsername,
                CreatedOn = x.CreatedOn
            }).CreatePagedResponseAsync(request);
}
