using System;

namespace TGS.Services.BackOffice.ApplicationServices;

public record GetAllGamesByGameProviderIdQuery(
    Guid GameProviderId);
