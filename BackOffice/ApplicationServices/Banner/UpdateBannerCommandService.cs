using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Banner;

internal class UpdateBannerCommandService(IBackOfficeRepository backOfficeRepository) : ICommandService<UpdateBannerCommand>
{
    private readonly IBackOfficeRepository _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));

    public ValueTask ExecuteAsync(UpdateBannerCommand command, CancellationToken cancellationToken = default)
    {
        var updateInfo = new BannerModifyInfo(
            command.BrandId,
            command.Title,
            command.BeginTime,
            command.EndTime,
            command.Localization);

        return _backOfficeRepository.UpdateBannerAsync(
            command.BannerId,
            updateInfo,
            command.Modifier);
    }
}
