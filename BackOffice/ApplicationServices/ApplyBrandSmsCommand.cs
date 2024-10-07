using System;

namespace TGS.Services.BackOffice.ApplicationServices;

public record ApplyBrandSmsCommand(
    Guid BrandId,
    Guid SmsSettingId,
    string Modifier);