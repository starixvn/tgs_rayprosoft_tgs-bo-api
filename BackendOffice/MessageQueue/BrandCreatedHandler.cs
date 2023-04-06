using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandService.Protos;
using Tgs.MessageQueue;
using TGS.Services.BackOffice.ApplicationServices;
using Tgs.ServiceUtility;

namespace TGS.Apis.BackendOffice.MessageQueue;

public class BrandCreatedHandler : IMessageHandler
{
    private readonly ICommandService<ProcessBrandCreatedCommand> _commandService;

    public BrandCreatedHandler(
        ICommandService<ProcessBrandCreatedCommand> commandService)
    {
        _commandService = commandService;
    }

    public ValueTask HandleAsync(
        ReadOnlyMemory<byte> data, 
        CancellationToken cancellationToken = default)
    {
        var request = BrandCreatedEvent.Parser.ParseFrom(data.ToArray());

        var command = new ProcessBrandCreatedCommand(request.BrandId);

        return _commandService.ExecuteAsync(command, cancellationToken);
    }
}