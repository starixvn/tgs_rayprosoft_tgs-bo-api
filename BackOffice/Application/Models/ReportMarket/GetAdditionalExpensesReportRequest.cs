using System;

namespace TGS.Services.BackOffice.Application.Models.ReportMarket;

public record GetAdditionalExpensesReportRequest
{
    public Guid BrandId { get; set; }

    public DateTimeOffset ReportDate { get; set; }
}
