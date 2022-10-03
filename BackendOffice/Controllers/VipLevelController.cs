using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Paging;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.VipLevel;
using TGS.Services.BackOffice.Application.Services;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VipLevelController : BaseContextController
{
    private readonly IMediator _mediator;

    public VipLevelController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Description("Create a vip level")]
    public async Task<MessageResponse> CreateAsync(
        [FromServices] IQueryService<AllVipLevelsQuery, VipLevelInfo> queryService,
        [FromServices] IVipLevelServiceClient vipLevelServiceClient,
        [FromBody] CreateVipLevelRequest request)
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

        var createInfo = new VipLevelCreateInfo(
            request.Level,
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
            request.VirtualSportsRebate);

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
    public async Task<PagedResponse<VipLevelModel>> GetListAsync(
        [FromServices] IQueryService<AllVipLevelsQuery, VipLevelInfo> queryService,
        [FromQuery] GetVipLevelsRequest request)
    {
        var query = new AllVipLevelsQuery(request.BrandId.Value);
        var vipLevels = queryService.QueryAsync(query, HttpContext.RequestAborted);

        var totalCount = await vipLevels
            .CountAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var paging = new Paging<VipLevelInfo>()
            .MapProperty("level", info => info.Level);

        paging.Page = request.Page;
        paging.RowCount = request.Rows;

        paging.SortByPropertyName(request.SortBy, false);

        vipLevels = vipLevels
            .ApplyOrderBy(paging)
            .ApplyPaging(paging);

        return new PagedResponse<VipLevelModel>
        {
            Valid = true,
            TotalCount = totalCount,
            Items = await vipLevels
                .Select(info => new VipLevelModel
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
                    VirtualSportsRebate = info.VirtualSportsRebate
                })
                .ToArrayAsync(HttpContext.RequestAborted)
                .ConfigureAwait(false)
        };
    }

    [HttpPut("{id}")]
    [Description("Update a vip level")]
    public async Task<MessageResponse> UpdateAsync(
        [FromServices] IVipLevelServiceClient vipLevelServiceClient,
        Guid id,
        [FromBody] UpdateVipLevelRequest request)
    {
        var updateInfo = VipLevelUpdateInfo.Default with
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
            VirtualSportsRebate = request.VirtualSportsRebate
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
