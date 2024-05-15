using System;
using Tgs.BrandCertificate.Clients;

namespace TGS.Services.BackOffice.ApplicationServices;
public record AddBrandSocialMediaSettingCommand(
    Guid BrandId,
    AddSocialMediaSetting Setting,
    string Modifier);
