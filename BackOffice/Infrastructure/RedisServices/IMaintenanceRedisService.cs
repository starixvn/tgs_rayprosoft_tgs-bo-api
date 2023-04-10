using System;
using System.Threading.Tasks;

namespace TGS.Services.BackOffice.Infrastructure.RedisServices;

public interface IMaintenanceRedisService
{
    ValueTask AddBrandMaintenanceAsync(Guid id);

    ValueTask AddBrandMaintenancesAsync(Guid[] ids);

    ValueTask AddLicenseeMaintenanceAsync(Guid id);

    ValueTask AddLicenseeMaintenancesAsync(Guid[] ids);

    ValueTask AddResellerMaintenanceAsync(Guid id);

    ValueTask AddResellerMaintenancesAsync(Guid[] ids);

    ValueTask ClearResellerMaintenanceAsync(Guid id);

    ValueTask ClearResellerMaintenancesAsync(Guid[] ids);

    ValueTask ClearBrandMaintenanceAsync(Guid id);

    ValueTask ClearBrandMaintenancesAsync(Guid[] ids);

    ValueTask ClearLicenseeMaintenanceAsync(Guid id);

    ValueTask ClearLicenseeMaintenancesAsync(Guid[] ids);

    ValueTask<string> GetBrandMaintenanceAsync(Guid id);

    ValueTask<string> GetLicenseeMaintenanceAsync(Guid id);

    ValueTask<string> GetResellerMaintenanceAsync(Guid ids);
}