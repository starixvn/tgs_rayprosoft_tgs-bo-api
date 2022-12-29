using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class AgentModel
{
    public decimal AccumulatedProfitSharing { get; init; }

    public AgentDividendConditionModel[] AgentDividendConditions { get; set; }

    public AgentConfigStatus AgentStatus { get; set; }

    public decimal Balance { get; set; }

    public AgentBankModel[] Banks { get; set; }

    public Guid BrandId { get; set; }

    public decimal CardGameRebatePercentage { get; init; }

    public decimal CommissionPercentage { get; set; }

    //佔成
    public decimal DividendPercentage { get; set; }

    public string Email { get; set; }

    public Guid Id { get; set; }

    public string InvitationCode { get; set; }

    public AgentLevelType LevelType { get; init; }

    public decimal LiveDealerRebatePercentage { get; init; }

    public decimal LotteryRebatePercentage { get; init; }

    public string ParentUsername { get; set; }

    public string Path { get; set; }

    public string Phone { get; set; }

    public int PlayerNumber { get; set; }

    public string QQ { get; set; }

    public string RealName { get; set; }

    public DateTimeOffset RegisteredOn { get; set; }

    public string Remarks { get; set; }

    public decimal SlotRebatePercentage { get; init; }

    public decimal SportsRebatePercentage { get; init; }

    public AgentStatus Status { get; set; }

    public int TeamNumber { get; set; }

    public int TotalCount { get; set; }

    public AgentType Type { get; set; }

    public string Username { get; set; }

    public decimal VirtualSportsRebatePercentage { get; init; }

    public string Wechat { get; set; }
}
