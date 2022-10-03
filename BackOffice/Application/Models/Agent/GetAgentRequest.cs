using System;
using MediatR;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class GetAgentRequest : IRequest<GetAgentResponse>
{
    public GetAgentRequest(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
}