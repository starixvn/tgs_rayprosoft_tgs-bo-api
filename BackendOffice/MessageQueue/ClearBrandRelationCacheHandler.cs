using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Apis.BackendOffice.MessageQueue;

public class ClearBrandRelationCacheHandler : IMessageHandler
{
    private readonly ICommandService<ClearBrandRelationCacheCommand> _commandService;

    public ClearBrandRelationCacheHandler(
        ICommandService<ClearBrandRelationCacheCommand> commandService)
    {
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
    }

    public ValueTask HandleAsync(
        ReadOnlyMemory<byte> data, 
        CancellationToken cancellationToken = default)
    {
        var command = new ClearBrandRelationCacheCommand();

        return _commandService.ExecuteAsync(command, cancellationToken);
    }
}