using System;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Services.BackOffice.Application.Models.Agent;

public class AgentRequest : BaseRequest
{
    public string Password { get; init; }
    public string RealName { get; init; }
    public string ParentUsername { get; init; }
    public MemberUpdateType UpdateType { get; init; }
    public AgentLevelType LevelType { get; init; }
    public AgentType Type { get; init; }
    public Guid BrandId { get; init; }
    public AgentStatus Status { get; init; }
    public decimal CommissionPercentage { get; init; } //佔成
    public decimal DividendPercentage { get; init; }
    public decimal LiveDealerRebatePercentage { get; init; }
    public decimal SlotRebatePercentage { get; init; }
    public decimal LotteryRebatePercentage { get; init; }
    public decimal CardGameRebatePercentage { get; init; }
    public decimal SportsRebatePercentage { get; init; }
    public decimal VirtualSportsRebatePercentage { get; init; }
    public string Remarks { get; init; }
    public AgentDividendConditionModel[] AgentDividendConditions { get; init; }
}