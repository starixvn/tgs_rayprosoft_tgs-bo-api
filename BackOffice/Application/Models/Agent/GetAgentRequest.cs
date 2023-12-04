using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class GetAgentRequest(Guid id) : IRequest<GetAgentResponse>
{
    public Guid Id { get; set; } = id;
}
