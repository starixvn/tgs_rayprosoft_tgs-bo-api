using System;

namespace TGS.Services.BackOffice.Application.Models.Aggregate;

public record GenerateGameProviderAggregateRequest
    (DateTimeOffset StartDate, Guid? GameProviderId)
{
    public string UserId { get; set; }
}
