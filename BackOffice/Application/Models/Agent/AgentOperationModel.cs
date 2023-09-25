using System;
using Tgs.Agent.Client.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class AgentOperationModel
{
    public string CreatedByUsername { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public Guid Id { get; set; }

    public AgentOperation Operation { get; set; }
}
