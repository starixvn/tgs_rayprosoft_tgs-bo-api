using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class DeleteAgentBankRequest : BaseRequest, IRequest<MessageResponse>
{
    public Guid Id { get; set; }

    public DeleteAgentBankRequest(Guid id)
    {
        Id = id;
    }
}
