using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class UpsertBrandPlayerContractDisplayCommandService : ICommandService<UpsertBrandPlayerContractDisplayCommand>
{
    public ValueTask ExecuteAsync(UpsertBrandPlayerContractDisplayCommand command, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }
}