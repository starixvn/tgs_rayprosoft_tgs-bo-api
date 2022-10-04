using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class ModifyBrandLogoInfoCommandService : ICommandService<ModifyBrandLogoInfoCommand>
{
    private readonly IBackOfficeRepository _backOfficeRepository;

    public ModifyBrandLogoInfoCommandService(IBackOfficeRepository backOfficeRepository)
    {
        _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));
    }

    public ValueTask ExecuteAsync(ModifyBrandLogoInfoCommand command, CancellationToken cancellationToken = default)
        => _backOfficeRepository.UpdateBrandLogoInfoAsync(
            new BrandLogoInfo(
                command.BrandId,
                command.DesktopLogoUrl,
                command.MobileLogoUrl,
                command.LoadingIconUrl),
            command.Modifier);
}
