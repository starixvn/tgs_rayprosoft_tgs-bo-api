using System;

namespace TGS.Services.BackOffice.Application.Models.Aggregate;

public record GenerateBrandGameProviderAggregateRequest
    (DateTimeOffset StartDate, Guid? BrandId)
{
    public string UserId { get; set; }
}
