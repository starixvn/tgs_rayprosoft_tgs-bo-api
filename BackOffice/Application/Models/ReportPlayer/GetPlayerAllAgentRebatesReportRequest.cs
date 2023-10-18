#nullable enable
using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportPlayer;

public class GetPlayerAllAgentRebatesReportRequest : PageableRequest
{
    public Guid BrandId { get; set; }
    public string? Username { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}
