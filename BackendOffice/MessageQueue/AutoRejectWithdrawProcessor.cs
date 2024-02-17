using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Commands.Withdraw;
using Valhalla.MessageQueue;

namespace TGS.Apis.BackendOffice.MessageQueue;

public class AutoRejectWithdrawProcessor(
    ICommandService<ReviewWithdrawFormCommand> reviewCommandService,
    ILogger<AutoRejectWithdrawProcessor> logger)
    : IMessageHandler<ReadOnlyMemory<byte>>
{
    private readonly ILogger<AutoRejectWithdrawProcessor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICommandService<ReviewWithdrawFormCommand> _reviewCommandService = reviewCommandService ?? throw new ArgumentNullException(nameof(reviewCommandService));

    public async ValueTask HandleAsync(
        string subject,
        ReadOnlyMemory<byte> data,
        IEnumerable<MessageHeaderValue> headerValues,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Receive AutoRejectWithdraw request.");

        var request = RejectDepositRequest.Parser.ParseFrom(data.Span);

        var formId = request.FormId.ToGuid();
        var brandId = request.BrandId.ToGuid();

        var command = new ReviewWithdrawFormCommand(
            brandId,
            formId,
            null,
            ReviewState.Reject,
            null,
            null,
            null,
            null);

        await _reviewCommandService
            .ExecuteAsync(command, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("AutoRejectWithdraw processing completed.");
    }
}
