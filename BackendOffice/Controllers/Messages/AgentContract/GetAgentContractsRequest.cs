using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.AgentContract;

public class GetAgentContractsRequest : PageableRequest
{
    public Guid AgentId { get; set; }
}
