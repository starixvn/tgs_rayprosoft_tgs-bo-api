using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class ChangeBannerStatusCommandService : ICommandService<ChangeBannerStatusCommand>
{
    private readonly IBackOfficeRepository _backOfficeRepository;

    public ChangeBannerStatusCommandService(IBackOfficeRepository backOfficeRepository)
    {
        _backOfficeRepository =
            backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));
    }

    public ValueTask ExecuteAsync(ChangeBannerStatusCommand command, CancellationToken cancellationToken = default)
        => _backOfficeRepository.ChangeStatusAsync(
            command.BannerId,
            command.Status,
            command.Modifier);
}
