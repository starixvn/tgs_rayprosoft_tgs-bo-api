using System;
using Microsoft.EntityFrameworkCore;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class AgentPathModel
{
    public Guid Id { get; set; }

    public Guid ParentAgentId { get; set; }

    public LTree Path { get; set; }

    public string Username { get; set; }
}
