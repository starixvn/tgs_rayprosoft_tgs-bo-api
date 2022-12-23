using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BetLimit;

public class UpdateBetLimitRequest : BetLimitRequest, IRequest<MessageResponse>
{
}
