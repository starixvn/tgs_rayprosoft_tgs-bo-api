using System;

namespace TGS.Services.BackOffice.Application.Models.VipLevel;

public class VipLevelRequest : BaseRequest
{
    public Guid Id { get; set; }
    public int Level { get; set; }
    public Guid BrandId { get; init; }
    public decimal LevelUpAccumulateBetAmount { get; init; }
    public decimal LevelUpAccumulateDeposit { get; init; }
    public decimal LevelKeepAccumulateBetAmount { get; init; }
    public decimal LevelKeepAccumulateDeposit { get; init; }
    public bool LevelKeepCondition { get; init; }
    public decimal BirthBenefit { get; init; }
    public decimal PromotionBenefit { get; init; }
    public decimal QuarterlyBenefit { get; init; }
    public decimal LiveDealerRebate { get; init; }
    public decimal SlotRebate { get; init; }
    public decimal LotteryRebate { get; init; }
    public decimal CardGameRebate { get; init; }
    public decimal SportsRebate { get; init; }
    public decimal VirtualSportsRebate { get; init; }
}