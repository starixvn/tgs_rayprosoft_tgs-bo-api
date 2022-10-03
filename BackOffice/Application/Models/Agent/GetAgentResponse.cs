using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class GetAgentResponse : MessageResponse
{
    public AgentModel Agent { get; set; }
}