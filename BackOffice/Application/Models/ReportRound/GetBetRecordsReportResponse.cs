#nullable enable

namespace TGS.Services.BackOffice.Application.Models.ReportRound;

public class GetBetRecordsReportResponse
{
    public BetRecordView[] Items { get; init; } = default!;

    public BetRecordTotalView Total { get; init; } = default!;

    public int TotalCount { get; init; }
}
