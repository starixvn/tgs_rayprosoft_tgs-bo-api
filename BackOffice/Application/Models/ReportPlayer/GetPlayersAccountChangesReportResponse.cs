using System.Collections.Generic;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportPlayer;

public class GetPlayersAccountChangesReportResponse : MessageResponse
{
    public IEnumerable<PlayersAccountChangeReportModel> Items { get; init; }

    public PlayersAccountChangesTotalAmountReportModel Total { get; init; }

    public int TotalCount { get; init; }
}
