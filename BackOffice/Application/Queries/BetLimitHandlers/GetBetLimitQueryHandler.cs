using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.BetLimit;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.BetLimitHandlers;

public class GetBetLimitQueryHandler : Handler<GetBetLimitRequest,
    GetBetLimitResponse>
{
    private readonly IBetLimitService _service;

    public GetBetLimitQueryHandler(IBetLimitService service, IHttpContextAccessor accessor) :
        base(accessor)
    {
        _service = service;
    }

    public override async Task<GetBetLimitResponse> Handle(
        GetBetLimitRequest request,
        CancellationToken cancellationToken)
    {
        var betLimit = await _service.GetBetLimitAsync(x => x.Id == request.Id);
        if (betLimit == null)
        {
            return CreateInvalidResponse<GetBetLimitResponse>(BrandGame.NotFound);
        }

        return new GetBetLimitResponse
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