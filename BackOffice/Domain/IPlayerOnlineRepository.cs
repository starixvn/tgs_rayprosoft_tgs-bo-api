using System;
using System.Threading.Tasks;

namespace TGS.Services.BackOffice.Domain;

public interface IPlayerOnlineRepository
{
    ValueTask<string[]> GetQueryAsync(Guid brandId);
}
