using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.PlayerIdentity;

internal class ReviewPlayerIdentityFormCommandService(IPlayerService playerService) : ICommandService<ReviewPlayerIdentityFormCommand>
{
    private readonly IPlayerService _playerService = playerService ?? throw new System.ArgumentNullException(nameof(playerService));

    public async ValueTask ExecuteAsync(ReviewPlayerIdentityFormCommand command, CancellationToken cancellationToken = default)
    {
        var formData = await _playerService
            .GetPlayerIdentityFormByIdAsync(command.FormId)
            .ConfigureAwait(false);

        if (formData == null)
            return;

        var formStatus = formData.Status;

        if (formStatus is PlayerIdentityReviewState.Approve or PlayerIdentityReviewState.Reject)
            return;

        await _playerService
            .SignOffPlayerIdentityFormAsync(
                command.BrandId,
                command.FormId,
                command.Status,
                command.ReviewUserId,
                command.ReviewUserName,
                command.Remark,
                command.RealName,
                command.IdNumber)
            .ConfigureAwait(false);
    }
}
