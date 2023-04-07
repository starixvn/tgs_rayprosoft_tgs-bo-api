using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class ClearBrandRelationCacheCommandService : ICommandService<ClearBrandRelationCacheCommand> 
{
    private readonly IClearBrandRelationCacheService _clearService;

    public ClearBrandRelationCacheCommandService(
        IClearBrandRelationCacheService clearService)
    {
        _clearService = clearService ?? throw new ArgumentNullException(nameof(clearService));
    }

    public ValueTask ExecuteAsync(
        ClearBrandRelationCacheCommand command,
        CancellationToken cancellationToken = default) 
        => _clearService.ClearAsync(cancellationToken);
}