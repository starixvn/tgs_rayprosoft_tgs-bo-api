using System;
using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class CreateAgentBankRequest : BaseRequest, IRequest<MessageResponse>
{
    public Guid AgentId { get; set; }

    public string BankAccount { get; set; }

    public string BankName { get; set; }

    public string RealName { get; set; }
}
