using System;
using MediatR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class UpdateAgentBankRequest : BaseRequest, IRequest<MessageResponse>
{
    public string BankAccount { get; set; }

    public string BankName { get; set; }

    public Guid Id { get; set; }

    public string RealName { get; set; }
}
