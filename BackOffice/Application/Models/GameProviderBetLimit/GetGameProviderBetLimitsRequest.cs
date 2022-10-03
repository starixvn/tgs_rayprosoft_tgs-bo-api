using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

public class GetGameProviderBetLimitsRequest : IRequest<GetGameProviderBetLimitsResponse>
{
    public Guid[] GameProviderIds { get; set; }
    public int BetLimitId { get; set; }
    public bool IsSports { get; set; }
}