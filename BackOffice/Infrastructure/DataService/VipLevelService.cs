using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.Application.Models.VipLevel;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IVipLevelService
{
    Task<VipLevel> GetVipLevelAsync(Expression<Func<VipLevel, bool>> predicate,
        Func<IQueryable<VipLevel>, IQueryable<VipLevel>> preQuery = null);

    IQueryable<VipLevel> GetVipLevelQueries(
        Func<IQueryable<VipLevel>, IQueryable<VipLevel>> preQuery = null,
        bool tracking = false);

    Task CreateVipLevelAsync(CreateVipLevelRequest request);
    Task UpdateVipLevelAsync(VipLevel language, UpdateVipLevelRequest request);
    Task<bool> ExistsAsync(Expression<Func<VipLevel, bool>> predicate);
}

public class VipLevelService : BaseRepositoryOperations<ITgsRepository>, IVipLevelService
{
    public VipLevelService(ITgsRepository tgsRepository) : base(
        tgsRepository)
    {
    }

    Task<VipLevel> IVipLevelService.GetVipLevelAsync(Expression<Func<VipLevel, bool>> predicate,
        Func<IQueryable<VipLevel>, IQueryable<VipLevel>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);

    IQueryable<VipLevel> IVipLevelService.GetVipLevelQueries(
        Func<IQueryable<VipLevel>, IQueryable<VipLevel>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);

    Task IVipLevelService.UpdateVipLevelAsync(
        VipLevel vipLevel, UpdateVipLevelRequest request)
    {
        vipLevel.LevelUpAccumulateBetAmount = request.LevelUpAccumulateBetAmount;
        vipLevel.LevelUpAccumulateDeposit = request.LevelUpAccumulateDeposit;
        vipLevel.LevelKeepAccumulateBetAmount = request.LevelKeepAccumulateBetAmount;
        vipLevel.LevelKeepAccumulateDeposit = request.LevelKeepAccumulateDeposit;
        vipLevel.LevelKeepCondition = request.LevelKeepCondition;
        vipLevel.BirthReward = request.BirthBenefit;
        vipLevel.PromotionReward = request.PromotionBenefit;
        vipLevel.QuarterlyReward = request.QuarterlyBenefit;
        vipLevel.LiveDealerRebatePercentage = request.LiveDealerRebate;
        vipLevel.SlotRebatePercentage = request.SlotRebate;
        vipLevel.LotteryRebatePercentage = request.LotteryRebate;
        vipLevel.CardGameRebatePercentage = request.CardGameRebate;
        vipLevel.SportsRebatePercentage = request.SportsRebate;
        vipLevel.VirtualSportsRebatePercentage = request.VirtualSportsRebate;
        vipLevel.UpdatedOn = DateTimeOffset.Now;

        return UpdateEntityAsync(vipLevel);
    }

    Task IVipLevelService.CreateVipLevelAsync(CreateVipLevelRequest request)
    {
        var vipLevel = new VipLevel
        {
            Level = request.Level,
            BrandId = request.BrandId,
            LevelUpAccumulateBetAmount = request.LevelUpAccumulateBetAmount,
            LevelUpAccumulateDeposit = request.LevelUpAccumulateDeposit,
            LevelKeepAccumulateBetAmount = request.LevelKeepAccumulateBetAmount,
            LevelKeepAccumulateDeposit = request.LevelKeepAccumulateDeposit,
            LevelKeepCondition = request.LevelKeepCondition,
            BirthReward = request.BirthBenefit,
            PromotionReward = request.PromotionBenefit,
            QuarterlyReward = request.QuarterlyBenefit,
            LiveDealerRebatePercentage = request.LiveDealerRebate,
            SlotRebatePercentage = request.SlotRebate,
            LotteryRebatePercentage = request.LotteryRebate,
            CardGameRebatePercentage = request.CardGameRebate,
            SportsRebatePercentage = request.SportsRebate,
            VirtualSportsRebatePercentage = request.VirtualSportsRebate,
            CreatedByUsername = request.EditorUsername,
            UpdatedByUsername = request.EditorUsername,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now
        };

        return CreateEntityAsync(vipLevel);
    }

    Task<bool> IVipLevelService.ExistsAsync(Expression<Func<VipLevel, bool>> predicate) =>
        ExistsAsync(predicate);
}