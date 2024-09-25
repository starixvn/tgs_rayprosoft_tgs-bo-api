using System;
using System.Threading.Tasks;

namespace TGS.Services.BackOffice.Domain;

public interface IMenuSettingRepository
{
    ValueTask<bool> SetAsync(string roleId, string menus);

    ValueTask<string> GetAsync(string roleId);
}