using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal class UpdatePromotionStatusCommandService : ICommandService<UpdatePromotionStatusCommand>
{
    private readonly IPromotionServiceClient _promotionServiceClient;

    public UpdatePromotionStatusCommandService(
        IPromotionServiceClient promotionServiceClient)
    {
        _promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
    }

    public ValueTask ExecuteAsync(
        UpdatePromotionStatusCommand command,
        CancellationToken cancellationToken = default)
        => _promotionServiceClient.SetPromotionStatusAsync(
            command.PromotionId,
            command.Status,
            command.Modifier, cancellationToken);
}