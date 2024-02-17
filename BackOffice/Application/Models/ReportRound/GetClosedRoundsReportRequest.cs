using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.ReportRound;

public class GetClosedRoundsReportRequest : PageableRequest
{
    public string ExternalRoundId { get; set; }

    public Guid? GameProviderId { get; set; }
}
