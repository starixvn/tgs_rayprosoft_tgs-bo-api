using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.GameProviderHandlers;

public class
    GetGameProviderBetLimitQueryHandler : Handler<GetGameProviderBetLimitRequest, GetGameProviderBetLimitResponse>
{
    private readonly IGameProviderBetLimitService _service;

    public GetGameProviderBetLimitQueryHandler(IGameProviderBetLimitService service, IHttpContextAccessor accessor)
        : base(accessor)
    {
        _service = service;
    }

    public override async Task<GetGameProviderBetLimitResponse> Handle(GetGameProviderBetLimitRequest request,
        CancellationToken cancellationToken)
    {
        var gameProviderBetLimit = await _service.GetGameProviderBetLimitAsync(x => x.Id == request.Id);

        if (gameProviderBetLimit == null)
        {
            return CreateInvalidResponse<GetGameProviderBetLimitResponse>(GameProviderBetLimit.NotFound);
        }

        return new()
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