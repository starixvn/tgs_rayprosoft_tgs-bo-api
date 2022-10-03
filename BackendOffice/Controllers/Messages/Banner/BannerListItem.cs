using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Banner;

public class BannerListItem
{
    public DateTimeOffset BeginTime { get; set; }

    public int DisplayOrder { get; set; }

    public DateTimeOffset? EndTime { get; set; }

    public Guid Id { get; set; }

    public bool IsEnabled { get; set; }

    public string Modifier { get; set; }

    public BannerFlowStatus Status { get; set; }

    public string Title { get; set; }

    public DateTimeOffset UpdatedOn { get; set; }
}
