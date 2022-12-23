using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class GetAgentRequest : IRequest<GetAgentResponse>
{
    public Guid Id { get; set; }

    public GetAgentRequest(Guid id)
    {
        Id = id;
    }
}
