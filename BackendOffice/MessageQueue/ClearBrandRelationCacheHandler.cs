using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices;
using Valhalla.MessageQueue;

namespace TGS.Apis.BackendOffice.MessageQueue;

public class ClearBrandRelationCacheHandler(
    ICommandService<ClearBrandRelationCacheCommand> commandService) : IMessageHandler
{
    private readonly ICommandService<ClearBrandRelationCacheCommand> _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

    public ValueTask HandleAsync(
        ReadOnlyMemory<byte> data, 
        CancellationToken cancellationToken = default)
    {
        var command = new ClearBrandRelationCacheCommand();

        return _commandService.ExecuteAsync(command, cancellationToken);
    }
}