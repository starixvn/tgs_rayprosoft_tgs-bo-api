using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class UpdateAgentBankRequest : BaseRequest, IRequest<MessageResponse>
{
    public Guid Id { get; set; }
    public string RealName { get; set; }
    public string BankName { get; set; }
    public string BankAccount { get; set; }
}