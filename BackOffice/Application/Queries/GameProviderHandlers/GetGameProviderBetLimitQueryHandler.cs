using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.GameProviderHandlers;

public class
    GetGameProviderBetLimitQueryHandler(IGameProviderBetLimitService service, IHttpContextAccessor accessor) : Handler<GetGameProviderBetLimitRequest, GetGameProviderBetLimitResponse>(accessor)
{
    private readonly IGameProviderBetLimitService _service = service;

    public override async Task<GetGameProviderBetLimitResponse> Handle(GetGameProviderBetLimitRequest request,
        CancellationToken cancellationToken)
    {
        var gameProviderBetLimit = await _service.GetGameProviderBetLimitAsync(x => x.Id == request.Id);

        return gameProviderBetLimit == null
            ? CreateInvalidResponse<GetGameProviderBetLimitResponse>(GameProviderBetLimit.NotFound)
            : new()
            {
                Valid = true,
                GameProviderBetLimit = new()
                {
                    Id = gameProviderBetLimit.Id,
                    CurrencyCode = gameProviderBetLimit.CurrencyCode,
                    MinBet = gameProviderBetLimit.MinBet,
                    MaxBet = gameProviderBetLimit.MaxBet,
                    MaxBetPerMatch = gameProviderBetLimit.MaxBetPerMatch,
                    ExternalSetting = gameProviderBetLimit.ExternalSetting
                }
            };
    }
}
