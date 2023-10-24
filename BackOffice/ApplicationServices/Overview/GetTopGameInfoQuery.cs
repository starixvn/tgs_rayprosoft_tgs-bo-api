using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.Overview;

public record GetTopGameInfoQuery(
    IEnumerable<Guid> GameProviderIds,
    IEnumerable<int> GameIds);
