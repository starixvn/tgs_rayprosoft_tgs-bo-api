using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BetLimit;

public class GetBetLimitsRequest : PageableRequest, IRequest<PagedResponse<BetLimitModel>>
{
    public string Keyword { get; set; }
}