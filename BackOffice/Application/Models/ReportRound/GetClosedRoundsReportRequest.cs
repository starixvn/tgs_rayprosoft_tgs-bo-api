using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportRound;

public class GetClosedRoundsReportRequest : PageableRequest
{
    public string ExternalRoundId { get; set; }

    public Guid? GameProviderId { get; set; }
}
