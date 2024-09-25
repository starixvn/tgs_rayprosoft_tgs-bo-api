using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using TGS.Services.BackOffice.Domain;

namespace TGS.BackOffice.Repositories;

internal class MenuSettingRepository(
    IDatabase database) : IMenuSettingRepository
{
    private readonly IDatabase _database = database ?? throw new ArgumentNullException(nameof(database));

    public async ValueTask<string> GetAsync(string roleId)
    {
        var key = GetKey(roleId);
        if (await database.KeyExistsAsync(key).ConfigureAwait(false))
        {
            var value = await database.StringGetAsync(key).ConfigureAwait(false);
            if (!value.IsNullOrEmpty)
            {
                return value;
            }
        }
        return string.Empty;
    }

    public async ValueTask<bool> SetAsync(string roleId, string menus)
        => await database.StringSetAsync(GetKey(roleId), menus);

    private string GetKey(string roleId) => $"s1:bo:menu:{roleId}";
}