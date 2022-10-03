using System;

namespace TGS.Services.BackOffice.Application.Models.Aggregate;

public record CreateBrandGameProviderAggregateAuditLogRequest(Guid UserId, Guid? BrandId);