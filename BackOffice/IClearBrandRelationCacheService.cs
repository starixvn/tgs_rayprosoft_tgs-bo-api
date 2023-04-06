using System.Threading;
using System.Threading.Tasks;

namespace TGS.Services.BackOffice;

public interface IClearBrandRelationCacheService
{
    ValueTask ClearAsync(CancellationToken cancellationToken = default);
}