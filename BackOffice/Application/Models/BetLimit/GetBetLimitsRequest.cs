using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.BetLimit;

public class GetBetLimitsRequest : PageableRequest, IRequest<PagedResponse<BetLimitModel>>
{
    public string Keyword { get; set; }
}
