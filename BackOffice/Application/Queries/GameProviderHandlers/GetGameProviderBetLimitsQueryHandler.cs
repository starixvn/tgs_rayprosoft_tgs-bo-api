using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.GameProviderHandlers;

public class
    GetGameProviderBetLimitsQueryHandler : Handler<GetGameProviderBetLimitsRequest,
        GetGameProviderBetLimitsResponse>
{
    private readonly IBetLimitService _betLimitService;
    private readonly IGameProviderBetLimitService _service;

    public GetGameProviderBetLimitsQueryHandler(IGameProviderBetLimitService service,
        IBetLimitService betLimitService, IHttpContextAccessor accessor)
        :
        base(accessor)
    {
        _service = service;
        _betLimitService = betLimitService;
    }

    public override async Task<GetGameProviderBetLimitsResponse> Handle(GetGameProviderBetLimitsRequest request,
        CancellationToken cancellationToken)
    {
        var betLimit = await _betLimitService.GetBetLimitAsync(x => request.BetLimitId == x.Id);

        var betLimits = await _service.GetGameProviderBetLimitQueries(q => q.Where(ResellerId,
                    r => r.GameProvider.Resellers.Select(x => x.ResellerId).Contains(ResellerId.Value))
                .Where(LicenseeId,
                    l => l.GameProvider.Licensees.Select(x => x.LicenseeId).Contains(LicenseeId.Value))
                .Where(BrandId, b => b.GameProvider.Brands.Select(x => x.BrandId).Contains(BrandId.Value))
                .Where(x => request.GameProviderIds.Contains(x.GameProviderId))
                .Where(betLimit != null, x =>
                    x.CurrencyCode == betLimit.CurrencyCode && betLimit.MinBet <= x.MinBet &&
                    betLimit.MaxBet >= x.MaxBet)
                .Where(betLimit != null && request.IsSports, x => betLimit.MaxBetPerMatch >= x.MaxBetPerMatch))
            .Select(x => new GameProviderBetLimitModel
            {
                Id = x.Id,
                CurrencyCode = x.CurrencyCode,
                MinBet = x.MinBet,
                MaxBet = x.MaxBet,
                MaxBetPerMatch = x.MaxBetPerMatch,
                ExternalSetting = x.ExternalSetting
            }).ToArrayAsync(cancellationToken);

        return new() {Valid = true, BetLimits = betLimits};
    }
}