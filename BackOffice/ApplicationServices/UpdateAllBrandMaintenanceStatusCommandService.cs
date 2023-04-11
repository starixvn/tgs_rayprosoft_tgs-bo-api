using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class UpdateAllBrandMaintenanceStatusCommandService : ICommandService<UpdateAllBrandMaintenanceStatusCommand>
{
    private readonly IQueryService<BrandListQuery, BrandListItem> _brandListQueryService;
    private readonly ICommandService<UpdateBrandStatusCommand> _updateStatusCommandService;

    public UpdateAllBrandMaintenanceStatusCommandService(
        IQueryService<BrandListQuery, BrandListItem> brandListQueryService,
        ICommandService<UpdateBrandStatusCommand> updateStatusCommandService)
    {
        _brandListQueryService = brandListQueryService ?? throw new ArgumentNullException(nameof(brandListQueryService));
        _updateStatusCommandService = updateStatusCommandService ?? throw new ArgumentNullException(nameof(updateStatusCommandService));
    }

    public async ValueTask ExecuteAsync(
        UpdateAllBrandMaintenanceStatusCommand command, 
        CancellationToken cancellationToken = default)
    {
        var brands = await _brandListQueryService
            .QueryAsync(
                new BrandListQuery(
                    Enumerable.Empty<Guid>(),
                    null,
                    BrandStatus.Active),
                cancellationToken)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);

        var tasks = brands.Select(brand => _updateStatusCommandService.ExecuteAsync(
            new UpdateBrandStatusCommand(
                brand.BrandId,
                BrandStatus.Maintenance,
                command.EditorUsername),
            cancellationToken).AsTask());

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
