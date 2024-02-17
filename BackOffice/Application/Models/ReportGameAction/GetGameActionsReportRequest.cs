using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.ReportGameAction;

public class GetGameActionsReportRequest : PageableRequest
{
    public Guid RoundId { get; init; }
}
