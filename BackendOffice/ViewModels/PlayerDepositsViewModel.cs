using System.Collections.Generic;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportPlayerDeposit;

#nullable enable

namespace TGS.Apis.BackendOffice.ViewModels;

public class PlayerDepositsViewModel : MessageResponse
{
    public IEnumerable<PlayerDepositReportModel> Items { get; init; } = default!;

    public PlayerDepositsSummaryViewModel Summary { get; init; } = default!;

    public int TotalCount { get; init; }
}
