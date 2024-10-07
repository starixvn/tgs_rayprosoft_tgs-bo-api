using System;

namespace TGS.Services.BackOffice.ApplicationServices;

public record AddBrandSmsCommand(
    Guid BrandId,
    Guid SmsSettingId);