using Tgs.Report.Client.Enum;

namespace TGS.Services.BackOffice.Application.Models.ReportPlayer
{
    public class PlayerAllAgentRebateDetailGameTypeReportModel
    {
        public AllAgentGameType AllAgentGameType { get; set; }
        public int ActivePlayerNumber { get; set; }
        public decimal RebatePercentage { get; set; }
    }
}
