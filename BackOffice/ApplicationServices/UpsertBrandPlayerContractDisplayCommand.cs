using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices;

public record UpsertBrandPlayerContractDisplayCommand(
    Guid BrandId,
    IDictionary<string, bool> UpdateTypes,
    string Modifier);