using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Banner;

internal class SetIsEanbleBannerCommandService(IBackOfficeRepository backOfficeRepository) : ICommandService<SetIsEanbleBannerCommand>
{
    private readonly IBackOfficeRepository _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));

    public ValueTask ExecuteAsync(SetIsEanbleBannerCommand command, CancellationToken cancellationToken = default)
        => _backOfficeRepository.SetIsEnableBannerAsync(
            command.BannerId,
            command.IsEnable,
            command.Modifier);
}
