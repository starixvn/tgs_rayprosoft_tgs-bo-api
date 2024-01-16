using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Paging;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VipLevelController : BaseContextController
{
    [HttpPost]
    [Description("Create a vip level")]
    public async Task<MessageResponse> CreateAsync(
        [FromServices] IQueryService<AllVipLevelsQuery, Tgs.VipLevelService.Common.ViewModels.VipLevelInfoViewModel> queryService,
        [FromServices] IVipLevelServiceClient vipLevelServiceClient,
        [FromBody] ModifyVipLevelViewModel request)
    {
        var lastVip = await queryService.QueryAsync(
            new AllVipLevelsQuery(request.BrandId),
            HttpContext.RequestAborted)
            .OrderByDescending(vip => vip.Level)
            .ThenByDescending(vip => vip.LevelUpAccumulateBetAmount)
            .ThenByDescending(vip => vip.LevelUpAccumulateDeposit)
            .FirstOrDefaultAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (lastVip != null
            && (lastVip.LevelUpAccumulateBetAmount >= request.LevelUpAccumulateBetAmount
                || lastVip.LevelUpAccumulateDeposit >= request.LevelUpAccumulateDeposit))
            return CreateInvalidResponse<MessageResponse>(string.Format(
                "Level up accumulate can't greater than next level (Bet Amount :{0}) (Deposit :{1})",
                lastVip.LevelUpAccumulateBetAmount,
                lastVip.LevelUpAccumulateDeposit));

        var createInfo = new Tgs.VipLevelService.Common.ViewModels.VipLevelCreateViewModel(
            request.LevelUpAccumulateBetAmount,
            request.LevelUpAccumulateDeposit,
            request.LevelKeepAccumulateBetAmount,
            request.LevelKeepAccumulateDeposit,
            request.LevelKeepCondition,
            request.BirthBenefit,
            request.PromotionBenefit,
            request.QuarterlyBenefit,
            request.LiveDealerRebate,
            request.SlotRebate,
            request.LotteryRebate,
            request.CardGameRebate,
            request.SportsRebate,
            request.VirtualSportsRebate,
            request.WithdrawTransactionsPerDay,
            request.WithdrawTotalAmountPerDay);

        _ = await vipLevelServiceClient.CreateVipLevelAsync(
            request.BrandId,
            createInfo,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = true
        };
    }

    [HttpGet]
    [Description("Get vip levels")]
    public async Task<PagedResponse<VipLevelViewModel>> GetListAsync(
        [FromServices] IQueryService<AllVipLevelsQuery, Tgs.VipLevelService.Common.ViewModels.VipLevelInfoViewModel> queryService,
        [FromQuery] GetVipLevelsRequest request)
    {
        var query = new AllVipLevelsQuery(request.BrandId.Value);
        var vipLevels = queryService.QueryAsync(query, HttpContext.RequestAborted);

        var totalCount = await vipLevels
            .CountAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var paging = new Paging<Tgs.VipLevelService.Common.ViewModels.VipLevelInfoViewModel>()
            .MapProperty("level", info => info.Level);

        paging.Page = request.Page;
        paging.RowCount = request.Rows;

        paging.SortByPropertyName(request.SortBy, false);

        vipLevels = vipLevels
            .ApplyOrderBy(paging)
            .ApplyPaging(paging);

        return new PagedResponse<VipLevelViewModel>
        {
            Valid = true,
            TotalCount = totalCount,
            Items = await vipLevels
                .Select(info => new VipLevelViewModel
                {
                    Id = info.Id,
                    Level = info.Level,
                    BrandId = info.BrandId,
                    LevelKeepAccumulateBetAmount = info.LevelKeepAccumulateBetAmount,
                    LevelKeepAccumulateDeposit = info.LevelKeepAccumulateDeposit,
                    LevelKeepCondition = info.LevelKeepCondition,
                    LevelUpAccumulateBetAmount = info.LevelUpAccumulateBetAmount,
                    LevelUpAccumulateDeposit = info.LevelUpAccumulateDeposit,
                    BirthBenefit = info.BirthBenefit,
                    PromotionBenefit = info.PromotionBenefit,
                    QuarterlyBenefit = info.QuarterlyBenefit,
                    LiveDealerRebate = info.LiveDealerRebate,
                    SlotRebate = info.SlotRebate,
                    LotteryRebate = info.LotteryRebate,
                    CardGameRebate = info.CardGameRebate,
                    SportsRebate = info.SportsRebate,
                    VirtualSportsRebate = info.VirtualSportsRebate,
                    WithdrawTotalAmountPerDay = info.WithdrawTotalAmountPerDay,
                    WithdrawTransactionsPerDay = info.WithdrawTransactionsPerDay
                })
                .ToArrayAsync(HttpContext.RequestAborted)
                .ConfigureAwait(false)
        };
    }

    [HttpPut("{id}")]
    [Description("Update a vip level")]
    public async Task<MessageResponse> UpdateAsync(
        [FromServices] IQueryService<AllVipLevelsQuery, Tgs.VipLevelService.Common.ViewModels.VipLevelInfoViewModel> queryService,
        [FromServices] IVipLevelServiceClient vipLevelServiceClient,
        Guid id,
        [FromBody] ModifyVipLevelViewModel request)
    {
        var allVip = await queryService.QueryAsync(
            new AllVipLevelsQuery(request.BrandId),
            HttpContext.RequestAborted)
            .OrderBy(vip => vip.Level)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var itemIndex = allVip.Select((info, idx) => (info, idx))
            .Where(t => t.info.Id == id)
            .Select(t => t.idx)
            .Single();

        if (itemIndex > 0
            && (allVip[itemIndex - 1].LevelUpAccumulateBetAmount >= request.LevelUpAccumulateBetAmount
                || allVip[itemIndex - 1].LevelUpAccumulateDeposit >= request.LevelUpAccumulateDeposit))
            return CreateInvalidResponse<MessageResponse>(string.Format(
                "Level up accumulate can't less than previous level (Bet Amount :{0}) (Deposit :{1})",
                allVip[itemIndex - 1].LevelUpAccumulateBetAmount,
                allVip[itemIndex - 1].LevelUpAccumulateDeposit));

        if (itemIndex < allVip.Length - 1
            && (allVip[itemIndex + 1].LevelUpAccumulateBetAmount <= request.LevelUpAccumulateBetAmount ||
            allVip[itemIndex + 1].LevelUpAccumulateDeposit <= request.LevelUpAccumulateDeposit))
            return CreateInvalidResponse<MessageResponse>(string.Format(
                "Level up accumulate can't greater than next level (Bet Amount :{0}) (Deposit :{1})",
                allVip[itemIndex + 1].LevelUpAccumulateBetAmount,
                allVip[itemIndex + 1].LevelUpAccumulateDeposit));

        var updateInfo = Tgs.VipLevelService.Common.ViewModels.VipLevelUpdateViewModel.Default with
        {
            Level = request.Level,
            LevelKeepAccumulateBetAmount = request.LevelKeepAccumulateBetAmount,
            LevelKeepAccumulateDeposit = request.LevelKeepAccumulateDeposit,
            LevelKeepCondition = request.LevelKeepCondition,
            LevelUpAccumulateBetAmount = request.LevelUpAccumulateBetAmount,
            LevelUpAccumulateDeposit = request.LevelUpAccumulateDeposit,
            BirthBenefit = request.BirthBenefit,
            PromotionBenefit = request.PromotionBenefit,
            QuarterlyBenefit = request.QuarterlyBenefit,
            LiveDealerRebate = request.LiveDealerRebate,
            SlotRebate = request.SlotRebate,
            LotteryRebate = request.LotteryRebate,
            CardGameRebate = request.CardGameRebate,
            SportsRebate = request.SportsRebate,
            VirtualSportsRebate = request.VirtualSportsRebate,
            WithdrawTransactionsPerDay = request.WithdrawTransactionsPerDay,
            WithdrawTotalAmountPerDay = request.WithdrawTotalAmountPerDay
        };

        await vipLevelServiceClient.UpdateVipLevelAsync(
            id,
            updateInfo,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = true
        };
    }
}
