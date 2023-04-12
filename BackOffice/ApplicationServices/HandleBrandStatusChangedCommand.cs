using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.ApplicationServices;

internal record HandleBrandStatusChangedCommand(
    Guid BrandId,
    BrandStatus Status);