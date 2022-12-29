using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.VipLevel;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.VipLevelHandlers;

public class UpdateVipLevelHandler : Handler<UpdateVipLevelRequest, MessageResponse>
{
    private readonly IVipLevelService _service;

    public UpdateVipLevelHandler(IVipLevelService service, IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
    }

    public override async Task<MessageResponse> Handle(UpdateVipLevelRequest request,
        CancellationToken cancellationToken)
    {
        var vipLevel = await _service.GetVipLevelAsync(q => q.Id == request.Id);

        if (vipLevel == null)
        {
            return CreateInvalidResponse<MessageResponse>(VipLevel.NotFound);
        }

        var previousVip =
            await _service.GetVipLevelAsync(x => x.BrandId == vipLevel.BrandId && x.Level == vipLevel.Level - 1);

        if (previousVip != null && (previousVip.LevelUpAccumulateBetAmount >= request.LevelUpAccumulateBetAmount ||
            previousVip.LevelUpAccumulateDeposit >= request.LevelUpAccumulateDeposit))
        {
            return CreateInvalidResponse<MessageResponse>(string.Format(
                VipLevel.LevelUpAccumulateLessThanPreviousLevel,
                previousVip.LevelUpAccumulateBetAmount, previousVip.LevelUpAccumulateDeposit));
        }

        var nextVip =
            await _service.GetVipLevelAsync(x => x.BrandId == vipLevel.BrandId && x.Level == vipLevel.Level + 1);

        if (nextVip != null && (nextVip.LevelUpAccumulateBetAmount <= request.LevelUpAccumulateBetAmount ||
            nextVip.LevelUpAccumulateDeposit <= request.LevelUpAccumulateDeposit))
        {
            return CreateInvalidResponse<MessageResponse>(string.Format(
                VipLevel.LevelUpAccumulateGreaterThanNextLevel,
                nextVip.LevelUpAccumulateBetAmount, nextVip.LevelUpAccumulateDeposit));
        }

        await _service.UpdateVipLevelAsync(vipLevel, request);

        return CreateValidResponse<MessageResponse>();
    }
}
