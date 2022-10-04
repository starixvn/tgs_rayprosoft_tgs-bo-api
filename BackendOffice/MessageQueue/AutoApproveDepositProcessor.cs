using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Apis.BackendOffice.MessageQueue;

public class AutoApproveDepositProcessor : IMessageHandler
{
    private readonly ILogger<AutoApproveDepositProcessor> _logger;
    private readonly ICommandService<ReviewDepositFormCommand> _reviewCommandService;

    public AutoApproveDepositProcessor(
        ICommandService<ReviewDepositFormCommand> reviewCommandService,
        ILogger<AutoApproveDepositProcessor> logger)
    {
        _reviewCommandService = reviewCommandService ?? throw new ArgumentNullException(nameof(reviewCommandService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
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
