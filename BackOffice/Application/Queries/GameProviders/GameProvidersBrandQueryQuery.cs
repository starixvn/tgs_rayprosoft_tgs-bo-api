using System;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

public record GameProvidersBrandQueryQuery(Guid? GameProviderId, Guid? Brand);
