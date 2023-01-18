using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices.Payment;

namespace TGS.Apis.BackendOffice.MessageQueue;

public class AutoRejectDepositProcessor : IMessageHandler
{
    private readonly ILogger<AutoRejectDepositProcessor> _logger;
    private readonly ICommandService<ReviewDepositFormCommand> _reviewCommandService;

    public AutoRejectDepositProcessor(
        ICommandService<ReviewDepositFormCommand> reviewCommandService,
        ILogger<AutoRejectDepositProcessor> logger)
    {
        _reviewCommandService = reviewCommandService ?? throw new ArgumentNullException(nameof(reviewCommandService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

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
