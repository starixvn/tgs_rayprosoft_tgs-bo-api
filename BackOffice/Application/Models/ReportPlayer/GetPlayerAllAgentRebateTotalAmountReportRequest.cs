#nullable enable
using System;

namespace TGS.Services.BackOffice.Application.Models.ReportPlayer
{
    public record GetPlayerAllAgentRebateTotalAmountReportRequest
    {
        public Guid BrandId { get; set; }
        public string? Username { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
