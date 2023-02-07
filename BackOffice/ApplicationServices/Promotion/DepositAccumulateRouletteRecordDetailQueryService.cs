using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.DepositAccumulateRoulette.Clients;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal class DepositAccumulateRouletteRecordDetailQueryService :
    IGetService<DepositAccumulateRouletteRecordDetailQuery, DepositAccumulateRouletteGameRecordDetailInfo>
{
    private readonly IPromotionServiceClient _promotionServiceClient;
    private readonly IDepositAccumulateRouletteServiceClient _rouletteServiceClient;

    public DepositAccumulateRouletteRecordDetailQueryService(
        IDepositAccumulateRouletteServiceClient rouletteServiceClient,
        IPromotionServiceClient promotionServiceClient)
    {
        _rouletteServiceClient = rouletteServiceClient ?? throw new ArgumentNullException(nameof(rouletteServiceClient));
        _promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
    }

    public async ValueTask<DepositAccumulateRouletteGameRecordDetailInfo> GetAsync(
        DepositAccumulateRouletteRecordDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        var record = (await _rouletteServiceClient.GetRouletteRecordAsync(
                query.BrandId,
                query.RecordId,
                cancellationToken)
            .ConfigureAwait(false))!;

        var promotionRule = (await _promotionServiceClient
            .GetPromotionRuleAsync<DepositAccumulateRouletteSettings>(
                record.PromotionId, cancellationToken)
            .ConfigureAwait(false))!;

        var rouletteItem = promotionRule.CampaignDetail.RouletteItems.ElementAt(record.ItemIndex);

        return new DepositAccumulateRouletteGameRecordDetailInfo(rouletteItem.Title);
    }
}
