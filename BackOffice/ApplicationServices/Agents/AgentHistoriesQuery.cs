using System;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

public record AgentHistoriesQuery(
    Guid AgentId,
    DateTime From,
    DateTime To,
    int Skip = 0);
