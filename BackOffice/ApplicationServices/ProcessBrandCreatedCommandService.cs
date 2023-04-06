using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class ProcessBrandCreatedCommandService : ICommandService<ProcessBrandCreatedCommand> 
{
    private readonly IClearBrandRelationCacheService _clearService;

    public ProcessBrandCreatedCommandService(
        IClearBrandRelationCacheService clearService)
    {
        _clearService = clearService ?? throw new ArgumentNullException(nameof(clearService));
    }

    public ValueTask ExecuteAsync(
        ProcessBrandCreatedCommand command,
        CancellationToken cancellationToken = default) 
        => _clearService.ClearAsync(cancellationToken);
}