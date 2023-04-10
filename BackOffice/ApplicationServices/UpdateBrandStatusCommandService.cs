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

internal class UpdateBrandStatusCommandService : ICommandService<UpdateBrandStatusCommand>
{
    private readonly IBrandServiceClient _brandServiceClient;
    private readonly IRedisCacheManager _redisCacheManager;
    private readonly IMaintenanceRedisService _maintenanceRedisService;

    public UpdateBrandStatusCommandService(
        IBrandServiceClient brandServiceClient,
        IRedisCacheManager redisCacheManager,
        IMaintenanceRedisService maintenanceRedisService)
    {
        _brandServiceClient = brandServiceClient ?? throw new ArgumentNullException(nameof(brandServiceClient));
        _redisCacheManager = redisCacheManager ?? throw new ArgumentNullException(nameof(redisCacheManager));
        _maintenanceRedisService = maintenanceRedisService ?? throw new ArgumentNullException(nameof(maintenanceRedisService));
    }

    public async ValueTask ExecuteAsync(
        UpdateBrandStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var status = command.Status switch
        {
            BrandStatus.Active => ClientBrandStatus.Active,
            BrandStatus.Archived => ClientBrandStatus.Archived,
            BrandStatus.Inactive => ClientBrandStatus.Inactive,
            BrandStatus.Maintenance => ClientBrandStatus.Maintenance,
            _ => throw new NotImplementedException()
        };

        await _brandServiceClient.UpdateBrandStatusAsync(
            command.BrandId,
            status, 
            command.Modifier,
            cancellationToken).ConfigureAwait(false);

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