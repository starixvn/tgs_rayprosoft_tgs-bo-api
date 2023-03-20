using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal record UpdatePromotionMainInfoCommandService : ICommandService<UpdatePromotionMainInfoCommand>
{
    private readonly IPromotionServiceClient _promotionServiceClient;
    private readonly IBrandServiceClient _brandServiceClient;

    private static readonly HashSet<string> _PromotionTypes = new(5)
    {
        "ReliefMoneyDaily",
        "ReliefMoneyWeekly",
        "ReliefMoneyMonthly",
        "TurnoverRebate",
        "TurnoverRebateWeekly"
    };

    public UpdatePromotionMainInfoCommandService(
        IPromotionServiceClient promotionServiceClient,
        IBrandServiceClient brandServiceClient)
    {
        _promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
        _brandServiceClient = brandServiceClient ?? throw new ArgumentNullException(nameof(brandServiceClient));
    }

    public async ValueTask ExecuteAsync(
        UpdatePromotionMainInfoCommand command,
        CancellationToken cancellationToken = default)
    {
        var promotion = await _promotionServiceClient.GetPromotionMainInfoAsync(
            command.PromotionId,
            cancellationToken).ConfigureAwait(false);

        if (promotion is null)
            return;

        var offset = TimeSpan.FromHours(8);

        if (_PromotionTypes.Contains(command.PromotionType))
        {
            var brandInfo = await _brandServiceClient.GetBrandInfoAsync(
                promotion!.BrandId, 
                cancellationToken).ConfigureAwait(false);

            offset = TimeSpan.FromMinutes(brandInfo!.TimeSpanMinutes!.Value);
        }

        var from = new DateTimeOffset(command.From.ToDateTime(TimeOnly.MinValue), offset).UtcDateTime;
        var to = new DateTimeOffset(command.To.ToDateTime(TimeOnly.MinValue), offset).UtcDateTime;

        var updateInfo = new CreatePromotionMainInfo
        {
            Name = command.Name,
            EventDuring = (from, to),
            PromotionType = command.PromotionType,
            IsAutoSent = command.IsAutoSent,
            PromotionLimitAmount = command.Unlimited ? null : command.TotalAmountLimit
        };

        await _promotionServiceClient.UpdatePromotionMainInfoAsync(
            command.PromotionId,
            updateInfo,
            command.Modifier,
            cancellationToken).ConfigureAwait(false);
    }
}
