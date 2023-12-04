using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Commands.Withdraw;
using Valhalla.MessageQueue;

namespace TGS.Apis.BackendOffice.MessageQueue;

public class AutoPaidWithdrawProcessor(ICommandService<PaidWithdrawFormCommand> paidService, ILogger<AutoPaidWithdrawProcessor> logger) : IMessageHandler
{
    private readonly ILogger<AutoPaidWithdrawProcessor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICommandService<PaidWithdrawFormCommand> _paidService = paidService ?? throw new ArgumentNullException(nameof(paidService));

    public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Receive AutoPaidWithdraw request.");

        var request = PaidWithdrawRequest.Parser.ParseFrom(data.Span);

        var formId = request.FormId.ToGuid();
        var brandId = request.BrandId.ToGuid();

        var command = new PaidWithdrawFormCommand(
            brandId,
            formId,
            null,
            null);

        await _paidService
            .ExecuteAsync(command, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("AutoPaidWithdraw processing completed.");
    }
}
