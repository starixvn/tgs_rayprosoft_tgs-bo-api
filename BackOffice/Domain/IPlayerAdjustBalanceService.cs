using System;
using System.Threading;
using System.Threading.Tasks;

namespace TGS.Services.BackOffice.Domain;

public interface IPlayerAdjustBalanceService
{
    ValueTask ProcessAdjustApplyAsync(
        PlayerBalanceAdjustmentApplyInfo applyInfo,
        Guid modifyUserId,
        string modifyUser,
        CancellationToken cancellationToken = default);

    ValueTask ProcessTransferApplyAsync(
        PlayerBalanceTransferApplyInfo applyInfo,
        Guid modifyUserId,
        CancellationToken cancellationToken = default);
}
