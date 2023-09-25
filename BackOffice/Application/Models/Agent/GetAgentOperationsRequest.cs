using System;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class GetAgentOperationsRequest
{
    public Guid BrandId { get; set; }

    public DateTimeOffset From { get; set; }

    public DateTimeOffset To { get; set; }
}
