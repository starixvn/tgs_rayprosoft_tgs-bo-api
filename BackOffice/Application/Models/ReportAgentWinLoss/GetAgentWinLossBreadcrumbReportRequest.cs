using System;
using TGS.Core.Models.Enums;

#nullable enable

namespace TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;

public class GetAgentWinLossBreadcrumbReportRequest
{
    public Guid BrandId { get; set; }
    public string? Username { get; init; } = default!;
    public AccountType UserType { get; init; }
    public Guid? RoundId { get; init; }
}
