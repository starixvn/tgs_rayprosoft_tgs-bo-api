using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class CreateAgentRequest : AgentRequest, IRequest<MessageResponse>
{
    public string Username { get; init; }
}
