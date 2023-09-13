using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandService.Protos;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;
using Valhalla.MessageQueue;

namespace TGS.Apis.BackendOffice.MessageQueue;

internal class ProcessOfflineTempUserBatchHandler : IMessageHandler
{
    private readonly ICommandService<GetPlayerCodeByBatchCommand> _commandService;

    public ProcessOfflineTempUserBatchHandler(
        ICommandService<GetPlayerCodeByBatchCommand> commandService)
    {
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
    }

    public ValueTask HandleAsync(
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default)
    {
        var request = ProcessOfflineTempUserBatchRequest.Parser.ParseFrom(data.Span);

        var command = new GetPlayerCodeByBatchCommand(request.BatchId);

        return _commandService.ExecuteAsync(command, cancellationToken);
    }
}