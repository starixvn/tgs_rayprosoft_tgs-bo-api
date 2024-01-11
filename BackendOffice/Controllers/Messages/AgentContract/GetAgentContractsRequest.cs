using System;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.AgentContract;

public class GetAgentContractsRequest : PageableRequest
{
    public Guid AgentId { get; set; }
}
