using System;
using Microsoft.EntityFrameworkCore;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class AgentModel
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string RealName { get; set; }
    public AgentStatus Status { get; set; }
    public AgentConfigStatus AgentStatus { get; set; }
    public AgentType Type { get; set; }
    public AgentLevelType LevelType { get; init; }
    public string InvitationCode { get; set; }
    public Guid BrandId { get; set; }
    public string ParentUsername { get; set; }
    public DateTimeOffset RegisteredOn { get; set; }
    public decimal Balance { get; set; }
    public decimal CommissionPercentage { get; set; } //佔成
    public decimal DividendPercentage { get; set; }
    public decimal LiveDealerRebatePercentage { get; init; }
    public decimal SlotRebatePercentage { get; init; }
    public decimal LotteryRebatePercentage { get; init; }
    public decimal CardGameRebatePercentage { get; init; }
    public decimal SportsRebatePercentage { get; init; }
    public decimal VirtualSportsRebatePercentage { get; init; }
    public decimal AccumulatedProfitSharing { get; init; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string QQ { get; set; }
    public string Wechat { get; set; }
    public string Remarks { get; set; }
    public int TeamNumber { get; set; }
    public int PlayerNumber { get; set; }
    public AgentBankModel[] Banks { get; set; }
    public int TotalCount { get; set; }
    public AgentDividendConditionModel[] AgentDividendConditions { get; set; }
    public string Path { get; set; }
}