using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class DeleteAgentBankRequest : BaseRequest, IRequest<MessageResponse>
{
    public DeleteAgentBankRequest(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
}