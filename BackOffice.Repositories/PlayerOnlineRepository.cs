using System;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using TGS.Services.BackOffice.Domain;

namespace TGS.BackOffice.Repositories;

internal class PlayerOnlineRepository(
    IDatabase database) : IPlayerOnlineRepository
{
    private const string PlayerOnlineKey = "playeronline";
    private readonly IDatabase _database = database ?? throw new ArgumentNullException(nameof(database));

    public async ValueTask<string[]> GetQueryAsync(Guid brandId)
    {
        var redisDatas = (string[])await _database.ScriptEvaluateAsync(LuaScript.Prepare(
               //Redis的keys模糊查询：
               " local res = redis.call('KEYS', @keypattern) " +
               " return res "), new { @keypattern = $"{PlayerOnlineKey}:{brandId}:*" })
            .ConfigureAwait(true);

        return redisDatas.Select(x => x.Split(':').Last()).ToArray();
    }
}
