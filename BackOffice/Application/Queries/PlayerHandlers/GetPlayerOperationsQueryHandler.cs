using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.PlayerHandlers;

public class
    GetPlayerOperationsQueryHandler : Handler<GetPlayerOperationRequest, PagedResponse<PlayerOperationModel>>
{
    private readonly IPlayerService _service;

    public GetPlayerOperationsQueryHandler(IPlayerService service, IHttpContextAccessor accessor) :
        base(accessor)
    {
        _service = service;
    }

    public override Task<PagedResponse<PlayerOperationModel>> Handle(GetPlayerOperationRequest request,
        CancellationToken cancellationToken) =>
        _service.GetPlayerOperationQueries(q => q.Where(p =>
                p.CreatedOn >= request.From && p.CreatedOn <= request.To && p.Username == request.PlayerCode))
            .Select(x => new PlayerOperationModel
            {
                CreatedOn = x.CreatedOn, CreatedByUsername = x.CreatedByUsername, Operation = x.Operation
            }).CreatePagedResponseAsync(request);
}