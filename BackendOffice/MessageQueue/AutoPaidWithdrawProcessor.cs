using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Commands.Withdraw;
using Valhalla.MessageQueue;

namespace TGS.Apis.BackendOffice.MessageQueue;

public class AutoPaidWithdrawProcessor(
    ICommandService<ReviewWithdrawFormCommand> reviewService,
    ILogger<AutoPaidWithdrawProcessor> logger)
    : IMessageHandler
{
    public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Receive AutoPaidWithdraw request.");

        var request = PaidWithdrawRequest.Parser.ParseFrom(data.Span);

        var formId = request.FormId.ToGuid();
        var brandId = request.BrandId.ToGuid();

        var command = new ReviewWithdrawFormCommand(
            brandId,
            formId,
            null,
            ReviewState.Paid,
            null,
            null,
            null);

        await reviewService
            .ExecuteAsync(command, cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation("AutoPaidWithdraw processing completed.");
    }
}
