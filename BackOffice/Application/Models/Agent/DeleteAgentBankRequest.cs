using System;
using MediatR;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class DeleteAgentBankRequest(Guid id) : BaseRequest, IRequest<MessageResponse>
{
    public Guid Id { get; set; } = id;
}
