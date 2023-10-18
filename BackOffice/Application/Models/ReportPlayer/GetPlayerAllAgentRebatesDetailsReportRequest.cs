using System;

namespace TGS.Services.BackOffice.Application.Models.ReportPlayer;

public record GetPlayerAllAgentRebatesDetailsReportRequest
{
    public Guid BrandId { get; set; }
    public string PlayerCode { get; set; } = string.Empty;
    public DateTime SettleDateTime { get; set; }
}
