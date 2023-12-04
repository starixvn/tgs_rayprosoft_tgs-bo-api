using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Services.PlayerBucket.Protos;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;
using Valhalla.MessageQueue;

namespace TGS.Apis.BackendOffice.MessageQueue;

public class CleanBucketsHandler(ICommandService<ProcessCleanBucketsRequestCommand> cleanService) : IMessageHandler
{
    private readonly ICommandService<ProcessCleanBucketsRequestCommand> _cleanService = cleanService ?? throw new ArgumentNullException(nameof(cleanService));

    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var eventData = CleanFormCreatedEvent.Parser.ParseFrom(data.Span);

        var command = new ProcessCleanBucketsRequestCommand(eventData.FormId);

        return _cleanService.ExecuteAsync(command, cancellationToken);
    }
}
