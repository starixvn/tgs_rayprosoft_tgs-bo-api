using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Banner;

internal class ChangeBannerOrderCommandService(IBackOfficeRepository backOfficeRepository) : ICommandService<ChangeBannerOrderCommand>
{
    private readonly IBackOfficeRepository _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));

    public ValueTask ExecuteAsync(ChangeBannerOrderCommand command, CancellationToken cancellationToken = default)
        => _backOfficeRepository.ChangeBannerOrderAsync(
            command.BannerId,
            command.OrderNumber,
            command.Modifier);
}
