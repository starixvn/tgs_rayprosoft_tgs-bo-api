using System;
using System.Collections.Generic;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal record PlayerInfosByUserNamesQuery(
    Guid BrandId,
    IEnumerable<string> UserNames);

