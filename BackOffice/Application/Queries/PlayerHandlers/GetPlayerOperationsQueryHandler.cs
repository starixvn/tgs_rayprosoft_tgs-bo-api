using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.PlayerHandlers;

public class GetPlayerOperationsQueryHandler(
    IPlayerService service,
    IHttpContextAccessor accessor)
    : Handler<GetPlayerOperationRequest, PagedResponse<PlayerOperationModel>>(accessor)
{

    public override Task<PagedResponse<PlayerOperationModel>> Handle(
        GetPlayerOperationRequest request,
        CancellationToken cancellationToken)
        => service.GetPlayerOperationQueries(q => q.Where(p =>
                p.CreatedOn >= request.From && p.CreatedOn <= request.To && p.Username == request.PlayerCode))
            .Select(x => new PlayerOperationModel
            {
                CreatedOn = x.CreatedOn,
                CreatedByUsername = x.CreatedByUsername,
                Operation = x.Operation
            })
            .CreatePagedResponseAsync(request);
}
