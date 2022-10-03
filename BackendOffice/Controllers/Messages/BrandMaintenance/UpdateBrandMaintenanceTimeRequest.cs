using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandMaintenance
{
    public class UpdateBrandMaintenanceTimeRequest
    {
        public TimeOnly BeginTime { get; set; }

        public TimeOnly EndTime { get; set; }
    }
}
