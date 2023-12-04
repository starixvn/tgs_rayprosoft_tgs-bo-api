using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Banner;

internal class ReviewBannerCommandService(IBackOfficeRepository backOfficeRepository) : ICommandService<ReviewBannerCommand>
{
    private readonly IBackOfficeRepository _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));

    public ValueTask ExecuteAsync(ReviewBannerCommand command, CancellationToken cancellationToken = default)
        => _backOfficeRepository.ReviewBannerAsync(
            command.BannerId,
            command.ReviewState,
            command.Remark,
            command.Reviewer);
}
