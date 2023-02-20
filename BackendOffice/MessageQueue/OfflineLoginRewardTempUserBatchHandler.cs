using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandService.Protos;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices.Promotion;

namespace TGS.Apis.BackendOffice.MessageQueue;

internal class OfflineLoginRewardTempUserBatchHandler : IMessageHandler
{
    private readonly ICommandService<ProcessOfflineLoginRewardTempUserBatchCommand> _commandService;

    public OfflineLoginRewardTempUserBatchHandler(
        ICommandService<ProcessOfflineLoginRewardTempUserBatchCommand> commandService)
    {
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
    }

    public ValueTask HandleAsync(
        ReadOnlyMemory<byte> data, 
        CancellationToken cancellationToken = default)
    {
        var request = ProcessOfflineLoginRewardTempUserBatchRequest.Parser.ParseFrom(data.Span);

        var command = new ProcessOfflineLoginRewardTempUserBatchCommand(request.BatchId);

        return _commandService.ExecuteAsync(command, cancellationToken);
    }
}