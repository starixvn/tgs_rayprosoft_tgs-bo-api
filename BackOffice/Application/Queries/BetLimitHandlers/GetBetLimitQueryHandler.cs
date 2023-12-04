using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.BetLimit;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.BetLimitHandlers;

public class GetBetLimitQueryHandler(IBetLimitService service, IHttpContextAccessor accessor) : Handler<GetBetLimitRequest,
    GetBetLimitResponse>(accessor)
{
    private readonly IBetLimitService _service = service;

    public override async Task<GetBetLimitResponse> Handle(
        GetBetLimitRequest request,
        CancellationToken cancellationToken)
    {
        var betLimit = await _service.GetBetLimitAsync(x => x.Id == request.Id);

        return betLimit == null
            ? CreateInvalidResponse<GetBetLimitResponse>(BrandGame.NotFound)
            : new GetBetLimitResponse
            {
                Valid = true,
                BetLimit = new BetLimitResponseModel
                {
                    Id = betLimit.Id,
                    MinBet = betLimit.MinBet,
                    MaxBet = betLimit.MaxBet,
                    MaxBetPerMatch = betLimit.MaxBetPerMatch
                }
            };
    }
}
