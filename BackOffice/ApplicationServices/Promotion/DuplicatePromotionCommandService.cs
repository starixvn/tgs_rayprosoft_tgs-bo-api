#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.BrandCertificate.Clients;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal class DuplicatePromotionCommandService(ISystemClock systemClock,
    IGetService<GetBrandInfoQuery, BrandInfo?> getBrandInfoService,
    IGetService<GetPromotionMainInfoQuery, PromotionMainInfo> getMainInfoService,
    IGetService<GetPromotionRuleQuery, PromotionRuleInfo> getRuleService,
    IGetService<GetPromotionContentQuery, PromotionContent> getContentService,
    IGetService<CreatePromotionMainInfoCommand, Guid> createMainInfoService,
    ICommandService<UpdatePromotionRuleCommand> updateRuleService,
    ICommandService<UpdatePromotionContentCommand> updateContentService,
    ICommandService<UpdatePromotionStatusCommand> updateStatusService,
    ILogger<DuplicatePromotionCommandService> logger) : IGetService<DuplicatePromotionCommand, Guid>
{
    private static readonly HashSet<string> _OfflinePromotionTypes = new(2)
    {
        "OfflineLoginReward",
        "OfflineDepositReward"
    };

    private static readonly HashSet<string> _PromotionTypes = new(5)
    {
        "ReliefMoneyDaily",
        "ReliefMoneyWeekly",
        "ReliefMoneyMonthly",
        "TurnoverRebate",
        "TurnoverRebateWeekly"
    };

    private readonly IGetService<CreatePromotionMainInfoCommand, Guid> _createMainInfoService = createMainInfoService ?? throw new ArgumentNullException(nameof(createMainInfoService));
    private readonly IGetService<GetBrandInfoQuery, BrandInfo?> _getBrandInfoService = getBrandInfoService ?? throw new ArgumentNullException(nameof(getBrandInfoService));
    private readonly IGetService<GetPromotionContentQuery, PromotionContent> _getContentService = getContentService ?? throw new ArgumentNullException(nameof(getContentService));
    private readonly IGetService<GetPromotionMainInfoQuery, PromotionMainInfo> _getMainInfoService = getMainInfoService ?? throw new ArgumentNullException(nameof(getMainInfoService));
    private readonly IGetService<GetPromotionRuleQuery, PromotionRuleInfo> _getRuleService = getRuleService ?? throw new ArgumentNullException(nameof(getRuleService));
    private readonly ILogger<DuplicatePromotionCommandService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ISystemClock _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
    private readonly ICommandService<UpdatePromotionContentCommand> _updateContentService = updateContentService ?? throw new ArgumentNullException(nameof(updateContentService));
    private readonly ICommandService<UpdatePromotionStatusCommand> _updateStatusService = updateStatusService ?? throw new ArgumentNullException(nameof(updateStatusService));
    private readonly ICommandService<UpdatePromotionRuleCommand> _updateRuleService = updateRuleService ?? throw new ArgumentNullException(nameof(updateRuleService));

    public async ValueTask<Guid> GetAsync(
        DuplicatePromotionCommand command,
        CancellationToken cancellationToken = default)
    {
        var mainInfo = await _getMainInfoService.GetAsync(
            new GetPromotionMainInfoQuery(command.BasePromotionId),
            cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Base promotion is not found");

        if (_OfflinePromotionTypes.Contains(mainInfo.PromotionType))
        {
            _logger.LogInformation("Can't duplicate promotion with type '{promotionType}', id: {promotionId}", mainInfo.PromotionType, mainInfo.Id);
            throw new InvalidOperationException("Can't duplicate offline promotion");
        }

        var rule = await _getRuleService.GetAsync(
            new GetPromotionRuleQuery(mainInfo.Id),
            cancellationToken).ConfigureAwait(false);

        var content = await _getContentService.GetAsync(
            new GetPromotionContentQuery(mainInfo.Id),
            cancellationToken).ConfigureAwait(false);

        var offset = TimeSpan.FromHours(8D);

        if (_PromotionTypes.Contains(mainInfo.PromotionType))
        {
            var brandInfo = await _getBrandInfoService.GetAsync(
                new GetBrandInfoQuery(mainInfo.BrandId),
                cancellationToken).ConfigureAwait(false);

            offset = TimeSpan.FromMinutes(brandInfo!.TimeSpanMinutes!.Value);
        }

        var currentDate = DateOnly.FromDateTime(_systemClock.UtcNow.ToOffset(offset).DateTime);
        var basePromotionFrom = DateOnly.FromDateTime(
            ((DateTimeOffset)mainInfo.EventDuring.From).ToOffset(offset).DateTime);
        var basePromotionTo = DateOnly.FromDateTime(
            ((DateTimeOffset)mainInfo.EventDuring.To).ToOffset(offset).DateTime);

        var from = basePromotionFrom >= currentDate ? basePromotionFrom.AddDays(1) : currentDate.AddDays(1);

        var interval = basePromotionTo.DayNumber - basePromotionFrom.DayNumber;
        var to = from.AddDays(interval);

        var createMainInfoCommand = new CreatePromotionMainInfoCommand(
            mainInfo.BrandId,
            mainInfo.Name,
            mainInfo.PromotionType,
            from,
            to,
            mainInfo.PromotionLimitAmount is null,
            mainInfo.PromotionLimitAmount,
            mainInfo.IsAutoSent,
            command.Modifier);

        var promotionId = await _createMainInfoService.GetAsync(
            createMainInfoCommand,
            cancellationToken).ConfigureAwait(false);

        var updateRuleCommand = new UpdatePromotionRuleCommand(
            promotionId,
            rule.CompaignDetail,
            rule.VipLevels,
            command.Modifier);

        await _updateRuleService.ExecuteAsync(
            updateRuleCommand,
            cancellationToken).ConfigureAwait(false);

        var updateContentCommand = new UpdatePromotionContentCommand(
            promotionId,
            content.SubSubject,
            content.Content,
            content.Localization,
            content.PersonalComputerBanner,
            content.MobileBanner,
            content.Convention,
            command.Modifier);

        await _updateContentService.ExecuteAsync(
            updateContentCommand,
            cancellationToken).ConfigureAwait(false);

        await _updateStatusService.ExecuteAsync(
            new UpdatePromotionStatusCommand(
                promotionId, 
                PromotionStatus.Disabled, 
                command.Modifier), 
            cancellationToken).ConfigureAwait(false);

        return promotionId;
    }
}
