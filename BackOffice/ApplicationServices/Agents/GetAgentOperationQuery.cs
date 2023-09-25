using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

public record GetAgentOperationQuery(
    Guid BrandId,
    Guid AgentId,
    DateTimeOffset From,
    DateTimeOffset To,
    PageableRequest PageableRequest);
