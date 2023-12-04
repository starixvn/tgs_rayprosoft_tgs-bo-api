using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Infrastructure.RedisServices;
using ClientBrandStatus = Tgs.BrandCertificate.BrandStatus;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class HandleBrandStatusChangedCommandService(
    IBrandServiceClient brandServiceClient,
    IRedisCacheManager redisCacheManager,
    IMaintenanceRedisService maintenanceRedisService) : ICommandService<HandleBrandStatusChangedCommand>
{
    private readonly IBrandServiceClient _brandServiceClient = brandServiceClient ?? throw new ArgumentNullException(nameof(brandServiceClient));
    private readonly IRedisCacheManager _redisCacheManager = redisCacheManager ?? throw new ArgumentNullException(nameof(redisCacheManager));
    private readonly IMaintenanceRedisService _maintenanceRedisService = maintenanceRedisService ?? throw new ArgumentNullException(nameof(maintenanceRedisService));

    public async ValueTask ExecuteAsync(
        HandleBrandStatusChangedCommand command,
        CancellationToken cancellationToken = default)
    {
        await _redisCacheManager.ClearCacheAsync(RedisCacheType.Brand, command.BrandId);

        if (command.Status is BrandStatus.Active or BrandStatus.Maintenance)
        {
            var brandStatus = await _brandServiceClient
                .GetBrandStatusAsync(command.BrandId, cancellationToken)
                .ConfigureAwait(false);

            if (brandStatus == ClientBrandStatus.Active)
                await _maintenanceRedisService.ClearBrandMaintenanceAsync(command.BrandId).ConfigureAwait(false);
            else
                await _maintenanceRedisService.AddBrandMaintenanceAsync(command.BrandId).ConfigureAwait(false);
        }
    }
}