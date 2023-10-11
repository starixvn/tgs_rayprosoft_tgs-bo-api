using System;

namespace TGS.Services.BackOffice.Application.Models.ReportPlayer
{
    public class PlayerAllAgentRebateReportModel
    {
        public string Username { get; set; } = string.Empty;
        public string PlayerCode { get; set; } = string.Empty;
        public int MaxActivePlayerNumber { get; set; }
        public decimal ValidBet { get; set; }
        public decimal Rebate { get; set; }
        public DateTimeOffset SettleDateTime { get; set; }
        public DateTimeOffset SentOn { get; set; }
    }
}
