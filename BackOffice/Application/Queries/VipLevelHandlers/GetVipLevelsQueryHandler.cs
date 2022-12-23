using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.VipLevel;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.VipLevelHandlers;

public class GetVipLevelsQueryHandler : Handler<GetVipLevelsRequest, PagedResponse<VipLevelModel>>
{
    private readonly IVipLevelService _service;

    public GetVipLevelsQueryHandler(IVipLevelService service, IHttpContextAccessor accessor)
        : base(accessor)
    {
        _service = service;
    }

    public override Task<PagedResponse<VipLevelModel>> Handle(GetVipLevelsRequest request,
        CancellationToken cancellationToken) =>
        _service.GetVipLevelQueries(q => q.Where(ResellerId != null,
                    r => r.Brand.Licensee.ResellerId == ResellerId)
                .Where(LicenseeId != null,
                    l => l.Brand.LicenseeId == LicenseeId)
                .Where(BrandId != null, b => b.BrandId == BrandId)
                .Where(request.BrandId, b => b.BrandId == request.BrandId))
            .Select(x => new VipLevelModel
            {
                Id = x.Id,
                Level = x.Level,
                BrandId = x.BrandId,
                LevelKeepAccumulateBetAmount = x.LevelKeepAccumulateBetAmount,
                LevelKeepAccumulateDeposit = x.LevelKeepAccumulateDeposit,
                LevelKeepCondition = x.LevelKeepCondition,
                LevelUpAccumulateBetAmount = x.LevelUpAccumulateBetAmount,
                LevelUpAccumulateDeposit = x.LevelUpAccumulateDeposit,
                BirthBenefit = x.BirthReward,
                PromotionBenefit = x.PromotionReward,
                QuarterlyBenefit = x.QuarterlyReward,
                LiveDealerRebate = x.LiveDealerRebatePercentage,
                SlotRebate = x.SlotRebatePercentage,
                LotteryRebate = x.LotteryRebatePercentage,
                CardGameRebate = x.CardGameRebatePercentage,
                SportsRebate = x.SportsRebatePercentage,
                VirtualSportsRebate = x.VirtualSportsRebatePercentage,
                CreatedByUsername = x.CreatedByUsername,
                UpdatedByUsername = x.UpdatedByUsername,
                CreatedOn = x.CreatedOn,
                UpdatedOn = x.UpdatedOn
            }).CreatePagedResponseAsync(request);
}
