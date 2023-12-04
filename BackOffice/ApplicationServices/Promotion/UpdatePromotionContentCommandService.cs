using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal class UpdatePromotionContentCommandService(
    IPromotionServiceClient promotionServiceClient) : ICommandService<UpdatePromotionContentCommand>
{
    private readonly IPromotionServiceClient _promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));

    public async ValueTask ExecuteAsync(
        UpdatePromotionContentCommand command,
        CancellationToken cancellationToken = default)
    {
        var content = new PromotionContent
        {
            Content = command.Content,
            Convention = command.Terms,
            Localization = command.Localization ?? JsonDocument.Parse("{}"),
            MobileBanner = command.MobileBanner,
            PersonalComputerBanner = command.DesktopBanner,
            SubSubject = command.SubSubject
        };

        await _promotionServiceClient.SetPromotionContentAsync(
            command.PromotionId,
            content,
            command.Modifier,
            cancellationToken).ConfigureAwait(false);
    }
}