using System;
using Tgs.Report.Client.Models;

namespace TGS.Services.BackOffice.Application.Models.ReportPlayer;

public record GetPlayerAllAgentRebateDetailGameTypesReportRequest
{
    public Guid BrandId { get; set; }
    public string PlayerCode { get; set; } = string.Empty;
    public DateTime SettleDateTime { get; set; }
}
