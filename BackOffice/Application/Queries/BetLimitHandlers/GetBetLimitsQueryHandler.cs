using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Services.BackOffice.Application.Models.BetLimit;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.BetLimitHandlers;

public class GetBetLimitsQueryHandler(
    IBetLimitService service,
    IHttpContextAccessor accessor)
    : Handler<GetBetLimitsRequest
    , PagedResponse<BetLimitModel>>(accessor)
{
    public override Task<PagedResponse<BetLimitModel>> Handle(
        GetBetLimitsRequest request,
        CancellationToken cancellationToken)
        => service.GetBetLimitQueries()
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
            })
            .CreatePagedResponseAsync(request);
}
