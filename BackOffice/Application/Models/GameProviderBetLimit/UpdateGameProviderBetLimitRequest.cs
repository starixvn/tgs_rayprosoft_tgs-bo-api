using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

public class UpdateGameProviderBetLimitRequest : GameProviderBetLimitRequest, IRequest<MessageResponse>
{
}