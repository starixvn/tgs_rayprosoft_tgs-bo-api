using System;

namespace TGS.Services.BackOffice.ApplicationServices;

public record RemoveBrandSmsCommand(
    Guid BrandId,
    Guid SmsSettingId);