using System;
using Tgs.Agent.Client.Models.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

public record SetAgentOneLayerCommand(
    Guid AgentId,
    string ParentUsername,
    decimal DividendPercentage,
    AgentType Type,    
    string ModifyUsername);
