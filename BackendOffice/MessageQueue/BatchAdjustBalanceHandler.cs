using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandService.Protos;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.ApplicationServices.BatchAdjust;
using Valhalla.MessageQueue;

namespace TGS.Apis.BackendOffice.MessageQueue;

public class BatchAdjustBalanceHandler(ICommandService<ProcessBatchCommand> commandService)
    : IMessageHandler<ReadOnlyMemory<byte>>
{
    private readonly ICommandService<ProcessBatchCommand> _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

    public ValueTask HandleAsync(
        string subject,
        ReadOnlyMemory<byte> data,
        IEnumerable<MessageHeaderValue> headerValues,
        CancellationToken cancellationToken = default)
    {
        var request = ProcessAdjustBalanceBatchRequest.Parser.ParseFrom(data.Span);

        var command = new ProcessBatchCommand(
            request.BrandId,
            request.BatchId,
            Enum.Parse<PlayerAdjustmentTransactionType>(request.AdjustTransferType),
            request.IncludeInNetProfit,
            request.ModifyUserId,
            request.ModifyUserName);

        return _commandService.ExecuteAsync(command, cancellationToken);
    }
}
