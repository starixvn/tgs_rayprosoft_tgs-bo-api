using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
    {
        var query = service.GetBetLimitQueries();

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();
            query = query.Where(p => p.CurrencyCode.ToLower().Contains(keyword) ||
                                     p.Id.ToString().Contains(keyword) ||
                                     p.UpdatedByUsername.ToLower().Contains(keyword) ||
                                     p.CreatedByUsername.ToLower().Contains(keyword) ||
                                     p.MaxBet.ToString().Contains(keyword) ||
                                     p.MinBet.ToString().Contains(keyword) ||
                                     p.MaxBetPerMatch.ToString().Contains(keyword));
        }

        return query.Select(x => new BetLimitModel
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
}
