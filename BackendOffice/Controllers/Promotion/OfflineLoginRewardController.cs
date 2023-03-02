using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Promotion;

[Route("api/offline-login-reward")]
[ApiController]
public class OfflineLoginRewardController : BaseContextController
{
    [HttpPut("{brandId}:create", Name = "CreateOfflineLoginRewardPromotion")]
    public async ValueTask<MessageResponse> CreatePromotionAsync(
        [FromServices] IGetService<GetProcessedUsersQuery, OfflineUsersResult> queryService,
        [FromServices] ICommandService<CreateOfflineLoginPromotionCommand> createPromotionService,
        [FromServices] IBrandServiceClient brandServiceClient,
        Guid brandId,
        [FromBody] CreateOfflineLoginRewardPromotionViewModel createViewModel)
    {
        var brandInfo = await brandServiceClient.GetBrandInfoAsync(brandId)
            .ConfigureAwait(false);

        if (brandInfo is null)
            return new MessageResponse
            {
                Valid = false,
                Message = "Brand not found."
            };

        var query = new GetProcessedUsersQuery(createViewModel.BatchId);

        var result = await queryService.GetAsync(
            query,
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (result is null || result.Matchs == 0)
            return new MessageResponse
            {
                Valid = false,
                Message = $"Can't find users of batch id '{createViewModel.BatchId}'"
            };

        var offset = TimeSpan.FromMinutes(brandInfo.TimeSpanMinutes ?? 480);
        var from = new DateTimeOffset(createViewModel.From.ToDateTime(TimeOnly.MinValue), offset).UtcDateTime;
        var to = new DateTimeOffset(createViewModel.To.ToDateTime(TimeOnly.MinValue), offset).UtcDateTime;

        var command = new CreateOfflineLoginPromotionCommand(
            brandId,
            createViewModel.Name,
            (from, to),
            new OfflineLoginSettings(
                createViewModel.BonusAmount,
                createViewModel.TurnoverMultiple,
                result.Players
                    .Where(player => player.PlayerCode is not null)
                    .Select(player => player.PlayerCode)),
            Username);

        await createPromotionService.ExecuteAsync(
            command,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = true
        };
    }
}
