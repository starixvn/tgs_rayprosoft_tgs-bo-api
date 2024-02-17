using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.BetLimit;

public class UpdateBetLimitRequest : BetLimitRequest, IRequest<MessageResponse>
{
}
