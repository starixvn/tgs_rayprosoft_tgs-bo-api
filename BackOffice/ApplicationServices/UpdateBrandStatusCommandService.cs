using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;
using ClientBrandStatus = Tgs.BrandCertificate.BrandStatus;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class UpdateBrandStatusCommandService(
    IBrandServiceClient brandServiceClient,
    ICommandService<HandleBrandStatusChangedCommand> statusChangedCommandService) : ICommandService<UpdateBrandStatusCommand>
{
    private readonly IBrandServiceClient _brandServiceClient = brandServiceClient ?? throw new ArgumentNullException(nameof(brandServiceClient));
    private readonly ICommandService<HandleBrandStatusChangedCommand> _statusChangedCommandService = statusChangedCommandService ?? throw new ArgumentNullException(nameof(statusChangedCommandService));

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

        await _statusChangedCommandService.ExecuteAsync(
            new HandleBrandStatusChangedCommand(command.BrandId, command.Status),
            cancellationToken).ConfigureAwait(false);
    }
}