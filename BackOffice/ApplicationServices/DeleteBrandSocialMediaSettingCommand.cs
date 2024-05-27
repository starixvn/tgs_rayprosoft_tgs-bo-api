using System;

namespace TGS.Services.BackOffice.ApplicationServices;
public record DeleteBrandSocialMediaSettingCommand(Guid BrandId, Guid SettingId);
