#nullable enable
using System;
using Tgs.Report.Client.Models;

namespace TGS.Services.BackOffice.Application.Models.ReportPlayer
{
    public record GetPlayerAllAgentRebatesReportRequest : PageableRequest
    {
        public Guid BrandId { get; set; }
        public string? Username { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
