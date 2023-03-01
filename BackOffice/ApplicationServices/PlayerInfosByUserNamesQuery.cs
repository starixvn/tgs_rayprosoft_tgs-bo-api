using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices;

internal record PlayerInfosByUserNamesQuery(
    Guid BrandId,
    IEnumerable<string> UserNames);

