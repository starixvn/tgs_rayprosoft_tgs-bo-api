using System;
using Tgs.Agent.Client.Models.Enums;
using TGS.Services.BackOffice.Application.Models.Agent;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

public record SetAgentMultipleLayerCommand(
    Guid AgentId,
    string ParentUsername,
    AgentDividendConditionModel[] AgentDividendConditions,
    AgentType Type,
    string ModifyUsername);
