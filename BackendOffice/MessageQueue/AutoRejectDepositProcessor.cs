using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using Valhalla.MessageQueue;

namespace TGS.Apis.BackendOffice.MessageQueue;

public class AutoRejectDepositProcessor(
    ICommandService<ReviewDepositFormCommand> reviewCommandService,
    ILogger<AutoRejectDepositProcessor> logger) : IMessageHandler
{
    private readonly ILogger<AutoRejectDepositProcessor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICommandService<ReviewDepositFormCommand> _reviewCommandService = reviewCommandService ?? throw new ArgumentNullException(nameof(reviewCommandService));

    public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive AutoRejectDeposit request.");

        var request = RejectDepositRequest.Parser.ParseFrom(data.Span);

        var formId = request.FormId.ToGuid();
        var brandId = request.BrandId.ToGuid();

        var command = new ReviewDepositFormCommand(
            brandId,
            formId,
            ReviewState.Reject,
            null,
            null,
            null,
            null);

        await _reviewCommandService
            .ExecuteAsync(command, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("AutoRejectDeposit processing completed.");
    }
}
