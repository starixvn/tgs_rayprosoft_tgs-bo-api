using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.ApplicationServices;

public record UpdateBrandStatusCommand(
    Guid BrandId,
    BrandStatus Status,
    string Modifier);