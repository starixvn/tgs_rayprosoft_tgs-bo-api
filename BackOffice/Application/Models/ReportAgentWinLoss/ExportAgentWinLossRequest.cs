using System;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;

public class ExportAgentWinLossRequest : PageableRequest
{
    public Guid BrandId { get; init; }
    public string Username { get; init; }
    public AccountType MemberType { get; init; }
    public bool DownLine { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}