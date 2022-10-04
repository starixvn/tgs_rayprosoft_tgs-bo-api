using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class CreateBannerCommandService : ICommandService<CreateBannerCommand>
{
    private readonly IBackOfficeRepository _backOfficeRepository;

    public CreateBannerCommandService(IBackOfficeRepository backOfficeRepository)
    {
        _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));
    }

    public async ValueTask ExecuteAsync(CreateBannerCommand command, CancellationToken cancellationToken = default)
    {
        var createInfo = new BannerModifyInfo(
            command.BrandId,
            command.Title,
            command.BeginTime,
            command.EndTime,
            command.Localization);

        await _backOfficeRepository
            .CreateBannerAsync(createInfo, command.Modifier)
            .ConfigureAwait(false);
    }
}
