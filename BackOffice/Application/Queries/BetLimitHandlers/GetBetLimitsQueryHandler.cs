using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BetLimit;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.BetLimitHandlers;

public class GetBetLimitsQueryHandler : Handler<GetBetLimitsRequest,
    PagedResponse<BetLimitModel>>
{
    private readonly IBetLimitService _service;

    public GetBetLimitsQueryHandler(IBetLimitService service, IHttpContextAccessor accessor) :
        base(accessor)
    {
        _service = service;
    }

    public override Task<PagedResponse<BetLimitModel>> Handle(
        GetBetLimitsRequest request,
        CancellationToken cancellationToken) =>
        _service.GetBetLimitQueries()
            .Select(x => new BetLimitModel
            {
                Id = x.Id,
                MinBet = x.MinBet,
                MaxBet = x.MaxBet,
                MaxBetPerMatch = x.MaxBetPerMatch,
                CurrencyCode = x.CurrencyCode,
                CreatedOn = x.CreatedOn,
                UpdatedOn = x.UpdatedOn,
                CreatedByUsername = x.CreatedByUsername,
                UpdatedByUsername = x.UpdatedByUsername
            }).CreatePagedResponseAsync(request);
}