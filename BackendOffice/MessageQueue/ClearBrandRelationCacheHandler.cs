using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandService.Protos;
using Tgs.MessageQueue;
using TGS.Services.BackOffice.ApplicationServices;
using Tgs.ServiceUtility;

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
        _ = ClearBrandRelationCacheEvent.Parser.ParseFrom(data.ToArray());

        var command = new ClearBrandRelationCacheCommand();

        return _commandService.ExecuteAsync(command, cancellationToken);
    }
}