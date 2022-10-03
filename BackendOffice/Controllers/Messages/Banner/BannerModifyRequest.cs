using System;
using System.Collections.Generic;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Banner;

public class BannerModifyRequest
{
    public DateTimeOffset BeginTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }

    public IDictionary<string, BannerContent> Localization { get; set; }

    public string Title { get; set; }
}
