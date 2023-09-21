using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.Overview;

public record GetBrandGameQuery(
    Guid BrandId,
    IEnumerable<int> GameIds);
