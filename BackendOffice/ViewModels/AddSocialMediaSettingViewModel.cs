using System;
using Tgs.BrandCertificate;

namespace TGS.Apis.BackendOffice.ViewModels;

public class AddSocialMediaSettingViewModel
{
    public required string Name { get; set; }

    public required SocialType Type { get; set; }

    public required Uri Url { get; set; }
}
