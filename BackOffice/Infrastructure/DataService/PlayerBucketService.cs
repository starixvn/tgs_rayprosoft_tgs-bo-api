using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IPlayerBucketService
{
    IQueryable<Core.DataServices.Tgs.Models.PlayerBucket> GetPlayerBucketQueries(
        Func<IQueryable<Core.DataServices.Tgs.Models.PlayerBucket>,
            IQueryable<Core.DataServices.Tgs.Models.PlayerBucket>> preQuery = null,
        bool tracking = false);

    ValueTask<PlayerBucketHistory[]> GetPlayerBucketHistoriesAsync(Guid playerBucketId);
}

public class PlayerBucketService : BaseRepositoryOperations<ITgsRepository>, IPlayerBucketService
{
    public PlayerBucketService(ITgsRepository tgsRepository) : base(tgsRepository)
    {
    }

    public async ValueTask<PlayerBucketHistory[]> GetPlayerBucketHistoriesAsync(Guid playerBucketId)
        => await GetQueriesAsync<PlayerBucketHistory>(q => q.Where(x => x.PlayerBucketId == playerBucketId), false)
            .ToArrayAsync();

    public IQueryable<Core.DataServices.Tgs.Models.PlayerBucket> GetPlayerBucketQueries(
        Func<IQueryable<Core.DataServices.Tgs.Models.PlayerBucket>,
            IQueryable<Core.DataServices.Tgs.Models.PlayerBucket>> preQuery,
        bool tracking) =>
        GetQueriesAsync(preQuery, tracking);
}