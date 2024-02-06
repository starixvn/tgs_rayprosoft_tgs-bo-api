using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using Valhalla.MessageQueue;

namespace TGS.Apis.BackendOffice.MessageQueue;

public class AutoApproveDepositProcessor(
    ICommandService<ReviewDepositFormCommand> reviewCommandService,
    ILogger<AutoApproveDepositProcessor> logger)
    : IMessageHandler<ReadOnlyMemory<byte>>
{
    private readonly ILogger<AutoApproveDepositProcessor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICommandService<ReviewDepositFormCommand> _reviewCommandService = reviewCommandService ?? throw new ArgumentNullException(nameof(reviewCommandService));

    public async ValueTask HandleAsync(
        string subject,
        ReadOnlyMemory<byte> data,
        IEnumerable<MessageHeaderValue> headerValues,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Receive AutoApproveDeposit request.");

        var request = ApproveDepositRequest.Parser.ParseFrom(data.Span);

        var formId = Guid.Parse(request.FormId);
        var brandId = Guid.Parse(request.BrandId);

        var command = new ReviewDepositFormCommand(
            brandId,
            formId,
            ReviewState.Approve,
            request.ActualAmount?.ToDecimal(),
            null,
            null,
            null);

        await _reviewCommandService
            .ExecuteAsync(command, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("AutoApproveDeposit processing completed.");
    }
}
