#nullable enable

using System;
using System.Collections.Generic;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.ApplicationServices;

public record BrandListQuery(
    IEnumerable<Guid> BrandIs,
    string? Keyword,
    BrandStatus? Status);