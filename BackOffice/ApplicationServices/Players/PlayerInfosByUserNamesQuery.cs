using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal record PlayerInfosByUserNamesQuery(
    Guid BrandId,
    IEnumerable<string> UserNames);

