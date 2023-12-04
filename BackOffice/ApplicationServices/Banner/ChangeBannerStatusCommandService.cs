using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Banner;

internal class ChangeBannerStatusCommandService(IBackOfficeRepository backOfficeRepository) : ICommandService<ChangeBannerStatusCommand>
{
    private readonly IBackOfficeRepository _backOfficeRepository =
        backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));

    public ValueTask ExecuteAsync(ChangeBannerStatusCommand command, CancellationToken cancellationToken = default)
        => _backOfficeRepository.ChangeStatusAsync(
            command.BannerId,
            command.Status,
            command.Modifier);
}
