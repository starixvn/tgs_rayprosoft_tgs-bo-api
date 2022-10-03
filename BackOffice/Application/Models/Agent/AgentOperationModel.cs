using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class AgentOperationModel
{
    public Guid Id { get; set; }
    public AgentOperation Operation { get; set; }
    public string CreatedByUsername { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
}