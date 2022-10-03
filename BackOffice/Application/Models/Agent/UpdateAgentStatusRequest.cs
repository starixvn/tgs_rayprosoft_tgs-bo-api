using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class UpdateAgentStatusRequest : BaseRequest, IRequest<MessageResponse>
{
}