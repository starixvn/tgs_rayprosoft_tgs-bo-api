using System;

namespace TGS.Services.BackOffice.ApplicationServices;

public record SetBrandEnableSmsCommand(
    Guid BrandId,
    bool EnableSms,
    string Modifier);