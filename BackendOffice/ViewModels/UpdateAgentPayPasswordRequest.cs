using System;

namespace TGS.Apis.BackendOffice.ViewModels;

public class UpdateAgentPayPasswordRequest
{
    public Guid AgentId { get; set; }

    public string PayPassword { get; set; }
}
