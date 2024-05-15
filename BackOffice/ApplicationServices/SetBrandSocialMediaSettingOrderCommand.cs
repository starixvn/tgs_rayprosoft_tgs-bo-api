using System;

namespace TGS.Services.BackOffice.ApplicationServices;
public record SetBrandSocialMediaSettingOrderCommand(
    Guid SettingId,
    int NewOrder,
    string Modifier);
