#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal class CreatePromotionMainInfoCommandService : IGetService<CreatePromotionMainInfoCommand, Guid>
{
    private static readonly HashSet<string> _PromotionTypes = new(5)
    {
        "ReliefMoneyDaily",
        "ReliefMoneyWeekly",
        "ReliefMoneyMonthly",
        "TurnoverRebate",
        "TurnoverRebateWeekly"
    };

    private readonly IGetService<GetBrandInfoQuery, BrandInfo?> _getBranInfoService;
    private readonly IPromotionServiceClient _promotionServiceClient;

    public CreatePromotionMainInfoCommandService(
        IPromotionServiceClient promotionServiceClient,
        IGetService<GetBrandInfoQuery, BrandInfo?> getBranInfoService)
    {
        _promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
        _getBranInfoService = getBranInfoService ?? throw new ArgumentNullException(nameof(getBranInfoService));
    }

    public async ValueTask<Guid> GetAsync(
        CreatePromotionMainInfoCommand command,
        CancellationToken cancellationToken = default)
    {
        var offset = TimeSpan.FromHours(8D);

        if (_PromotionTypes.Contains(command.PromotionType))
        {
            var brandInfo = await _getBranInfoService.GetAsync(
                new GetBrandInfoQuery(command.BrandId),
                cancellationToken).ConfigureAwait(false);

            offset = TimeSpan.FromMinutes(brandInfo!.TimeSpanMinutes!.Value);
        }

        var from = new DateTimeOffset(command.From.ToDateTime(TimeOnly.MinValue), offset).UtcDateTime;
        var to = new DateTimeOffset(command.To.ToDateTime(TimeOnly.MinValue), offset).UtcDateTime;

        var createInfo = new CreatePromotionMainInfo
        {
            Name = command.Name,
            EventDuring = (from, to),
            PromotionType = command.PromotionType,
            IsAutoSent = command.IsAutoSent,
            PromotionLimitAmount = command.Unlimited ? null : command.TotalAmountLimit
        };

        return await _promotionServiceClient.CreatePromotionAsync(
            command.BrandId,
            createInfo,
            command.Modifier,
            cancellationToken).ConfigureAwait(false);
    }
}
