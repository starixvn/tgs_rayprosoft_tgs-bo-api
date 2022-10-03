using System;

namespace TGS.Services.BackOffice.Application.Models.Aggregate;

public record CreateGameProviderAggregateAuditLogRequest(Guid UserId, Guid? GameProviderId);