using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandMaintenance
{
    public class UpdateAllBrandMaintenanceTimeRequest
    {
        public TimeOnly BeginTime { get; init; }

        public TimeOnly EndTime { get; init; }
    }
}
