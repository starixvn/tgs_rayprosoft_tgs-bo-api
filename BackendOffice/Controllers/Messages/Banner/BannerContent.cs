using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Banner;

public class BannerContent
{
    public Uri DesktopBannerUrl { get; set; }

    public Uri MobileBannerUrl { get; set; }

    public Uri NavigateUrl { get; set; }

    public string PictureAlt { get; set; }

    public string Title { get; set; }
}
