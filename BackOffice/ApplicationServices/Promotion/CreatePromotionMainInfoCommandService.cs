using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal class CreatePromotionMainInfoCommandService : IGetService<CreatePromotionMainInfoCommand, Guid>
{
    private readonly IPromotionServiceClient _promotionServiceClient;
    private readonly IBrandServiceClient _brandServiceClient;
    
    private readonly ImmutableArray<string> _promotionTypes = new[]
    {
        "ReliefMoneyDaily",
        "ReliefMoneyWeekly",
        "ReliefMoneyMonthly",
        "TurnoverRebate",
        "TurnoverRebateWeekly"
    }.ToImmutableArray();

    public CreatePromotionMainInfoCommandService(
        IPromotionServiceClient promotionServiceClient,
        IBrandServiceClient brandServiceClient)
    {
        _promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
        _brandServiceClient = brandServiceClient ?? throw new ArgumentNullException(nameof(brandServiceClient));
    }

    public async ValueTask<Guid> GetAsync(
        CreatePromotionMainInfoCommand command, 
        CancellationToken cancellationToken = default)
    {
        var timeSpan = TimeSpan.FromHours(8);
       
        if (_promotionTypes.Contains(command.PromotionType))
        {
            var brandInfo = await _brandServiceClient.GetBrandInfoAsync(command.BrandId, cancellationToken)
                .ConfigureAwait(false);
            
            timeSpan = TimeSpan.FromMinutes(brandInfo!.TimeSpanMinutes!.Value);
        }

        var from = new DateTimeOffset(command.From.ToDateTime(TimeOnly.MinValue), timeSpan).UtcDateTime;
        var to = new DateTimeOffset(command.To.ToDateTime(TimeOnly.MinValue), timeSpan).UtcDateTime;
        
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