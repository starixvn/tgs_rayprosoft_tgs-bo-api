using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class CreateAgentRequest : AgentRequest, IRequest<MessageResponse>
{
    public string Username { get; init; }
}
