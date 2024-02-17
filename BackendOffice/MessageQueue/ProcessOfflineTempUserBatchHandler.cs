using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandService.Protos;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;
using Valhalla.MessageQueue;

namespace TGS.Apis.BackendOffice.MessageQueue;

internal class ProcessOfflineTempUserBatchHandler(
    ICommandService<GetPlayerCodeByBatchCommand> commandService)
    : IMessageHandler<ReadOnlyMemory<byte>>
{
    private readonly ICommandService<GetPlayerCodeByBatchCommand> _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

    public ValueTask HandleAsync(
        string subject,
        ReadOnlyMemory<byte> data,
        IEnumerable<MessageHeaderValue> headerValues,
        CancellationToken cancellationToken = default)
    {
        var request = ProcessOfflineTempUserBatchRequest.Parser.ParseFrom(data.Span);

        var command = new GetPlayerCodeByBatchCommand(request.BatchId);

        return _commandService.ExecuteAsync(command, cancellationToken);
    }
}
