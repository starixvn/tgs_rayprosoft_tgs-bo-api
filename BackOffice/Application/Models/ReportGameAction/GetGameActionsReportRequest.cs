using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportGameAction;

public class GetGameActionsReportRequest : PageableRequest
{
    public Guid RoundId { get; init; }
}