using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BetLimit;

public class CreateBetLimitRequest : BetLimitRequest, IRequest<MessageResponse>
{
}