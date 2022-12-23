using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

public class GetGameProviderBetLimitsRequest : IRequest<GetGameProviderBetLimitsResponse>
{
    public int BetLimitId { get; set; }

    public Guid[] GameProviderIds { get; set; }

    public bool IsSports { get; set; }
}
