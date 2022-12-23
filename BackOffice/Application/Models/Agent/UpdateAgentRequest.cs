using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class UpdateAgentRequest : AgentRequest, IRequest<MessageResponse>
{
    public string Email { get; init; }

    public Guid Id { get; set; }

    public string Phone { get; init; }

    public string QQ { get; init; }

    public string Wechat { get; init; }
}
