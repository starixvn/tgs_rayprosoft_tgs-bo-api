using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.AgentAdjust;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.AgentAdjustHandlers;

public class GetAgentAdjustsQueryHandler : Handler<GetAdjustAgentsRequest, PagedResponse<AdjustAgentModel>>
{
    private readonly IAgentService _service;

    public GetAgentAdjustsQueryHandler(IAgentService service, IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
    }

    public override Task<PagedResponse<AdjustAgentModel>> Handle(GetAdjustAgentsRequest request,
        CancellationToken cancellationToken) =>
        _service.GetAgentQueries(q => q.Where(ResellerId, r => r.Brand.Licensee.ResellerId == ResellerId)
                .Where(LicenseeId, l => l.Brand.LicenseeId == LicenseeId)
                .Where(BrandId, l => l.Id == BrandId)
                .Where(x => x.BrandId == request.BrandId)
                .Where(request.Status, x => x.Status == request.Status)
                .Where(request.LevelType, x => x.LevelType == request.LevelType)
                .Where(request.Username, x => EF.Functions.ILike(x.Username, $"%{request.Username}%"))
            )
            .Select(x => new AdjustAgentModel
            {
                Username = x.Username,
                Type = x.Type,
                LevelType = x.LevelType,
                Balance = x.Wallets.FirstOrDefault().Balance
            }).CreatePagedResponseAsync(request);
}
